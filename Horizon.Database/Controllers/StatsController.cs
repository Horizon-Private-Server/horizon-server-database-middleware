using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Horizon.Database.DTO;
using Horizon.Database.Entities;
using Horizon.Database.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Horizon.Database.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        private IAuthService authService;
        public StatsController(Ratchet_DeadlockedContext _db, IAuthService _authService)
        {
            db = _db;
            authService = _authService;
        }

        [Authorize("database")]
        [HttpGet, Route("initStats")]
        public async Task<dynamic> initStats(int AccountId)
        {
            AccountController ac = new AccountController(db, authService);
            AccountDTO existingAcc = await ac.getAccount(AccountId);

            if (existingAcc == null)
                return BadRequest($"Account Id {AccountId} doesn't exist.");

            if (existingAcc.AccountWideStats.Count() > 0)
                return StatusCode(403, "This account already has stats.");

            List<AccountStat> newStats = (from ds in db.DimStats
                                          select new AccountStat()
                                          {
                                              AccountId = existingAcc.AccountId,
                                              StatId = ds.StatId,
                                              StatValue = ds.DefaultValue
                                          }).ToList();
            db.AccountStat.AddRange(newStats);
            List<AccountCustomStat> newCustomStats = (from ds in db.DimCustomStats
                                                      where (ds.AppId == 0 || ds.AppId == existingAcc.AppId)
                                                      select new AccountCustomStat()
                                                      {
                                                          AccountId = existingAcc.AccountId,
                                                          StatId = ds.StatId,
                                                          StatValue = ds.DefaultValue
                                                      }).ToList();
            db.AccountCustomStat.AddRange(newCustomStats);
            db.SaveChanges();

            return Ok("Stats Created");
        }

        [Authorize("database")]
        [HttpGet, Route("validateStats")]
        public async Task<dynamic> validateStats()
        {
            db.Database.ExecuteSqlRaw("EXEC [dbo].[SyncAccountCustomStats]");
            db.Database.ExecuteSqlRaw("EXEC [dbo].[SyncClanCustomStats]");
            return Ok("Stats Validated");
        }

        [Authorize("database")]
        [HttpGet, Route("getStats")]
        public async Task<dynamic> getStats(int AccountId)
        {
            DateTime modifiedDt = DateTime.UtcNow;
            List<AccountStat> playerStats = db.AccountStat.Where(s => s.AccountId == AccountId).OrderBy(s => s.StatId).Select(s => s).ToList();

            return new StatPostDTO()
            {
                AccountId = AccountId,
                stats = playerStats.Select(x => x.StatValue).ToList()
            };
        }

        [Authorize("database")]
        [HttpGet, Route("getClanStats")]
        public async Task<dynamic> getClanStats(int ClanId)
        {
            DateTime modifiedDt = DateTime.UtcNow;
            List<ClanStat> clanStats = db.ClanStat.Where(s => s.ClanId == ClanId).OrderBy(s => s.StatId).Select(s => s).ToList();

            int badStats = clanStats.Where(s => s.StatValue < 0).Count();
            if (badStats > 0)
                return BadRequest("Found a negative stat in array. Can't have those!");

            return new ClanStatPostDTO()
            {
                ClanId = ClanId,
                stats = clanStats.Select(x => x.StatValue).ToList()
            };
        }

        [Authorize]
        [HttpGet, Route("getPlayerLeaderboardIndex")]
        public async Task<dynamic> getPlayerLeaderboardIndex(int AccountId, int StatId, int AppId)
        {
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == AppId
                                    select a.AppId).ToList();


            List<AccountStat> stats = db.AccountStat.Where(s => s.Account.IsActive == true && s.StatId == StatId && app_ids_in_group.Contains(s.Account.AppId ?? -1)).OrderByDescending(s => s.StatValue).ThenBy(s => s.AccountId).ToList();
            AccountStat statForAccount = stats.Where(s => s.AccountId == AccountId).FirstOrDefault();
            Account acc = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            AccountController ac = new AccountController(db, authService);
            int totalAccounts = await ac.getActiveAccountCountByAppId((int)acc.AppId);
            if (acc.IsActive == true)
            {
                return new LeaderboardDTO()
                {
                    TotalRankedAccounts = totalAccounts,
                    AccountId = AccountId,
                    Index = stats.IndexOf(statForAccount),
                    StartIndex = stats.IndexOf(statForAccount),
                    StatValue = statForAccount.StatValue,
                    AccountName = acc.AccountName,
                    MediusStats = acc.MediusStats
                };
            }
            return StatusCode(400, $"Account {AccountId} is inactive.");
        }

        [Authorize]
        [HttpGet, Route("getPlayerLeaderboardIndexCustom")]
        public async Task<dynamic> getPlayerLeaderboardIndexCustom(int AccountId, int CustomStatId, int AppId, bool orderAsc = false)
        {
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == AppId
                                    select a.AppId).ToList();

            List<AccountCustomStat> stats = null;
            if (orderAsc)
                stats = db.AccountCustomStat.Where(s => s.Account.IsActive == true && s.StatValue > 0 && s.StatId == CustomStatId && app_ids_in_group.Contains(s.Account.AppId ?? -1)).OrderBy(s => s.StatValue).ThenBy(s => s.AccountId).ToList();
            else
                stats = db.AccountCustomStat.Where(s => s.Account.IsActive == true && s.StatId == CustomStatId && app_ids_in_group.Contains(s.Account.AppId ?? -1)).OrderByDescending(s => s.StatValue).ThenBy(s => s.AccountId).ToList();

            AccountCustomStat statForAccount = stats.Where(s => s.AccountId == AccountId).FirstOrDefault();
            Account acc = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            AccountController ac = new AccountController(db, authService);
            int totalAccounts = await ac.getActiveAccountCountByAppId((int)acc.AppId);
            if (acc.IsActive == true)
            {
                return new LeaderboardDTO()
                {
                    TotalRankedAccounts = totalAccounts,
                    AccountId = AccountId,
                    Index = statForAccount == null ? totalAccounts - 1 : stats.IndexOf(statForAccount),
                    StartIndex = statForAccount == null ? totalAccounts - 1 : stats.IndexOf(statForAccount),
                    StatValue = statForAccount?.StatValue ?? 0,
                    AccountName = acc.AccountName,
                    MediusStats = acc.MediusStats
                };
            }
            return StatusCode(400, $"Account {AccountId} is inactive.");
        }

        [Authorize]
        [HttpGet, Route("getClanLeaderboardIndex")]
        public async Task<dynamic> getClanLeaderboardIndex(int ClanId, int StatId, int AppId)
        {
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == AppId
                                    select a.AppId).ToList();

            List<ClanStat> stats = db.ClanStat.Where(s => s.Clan.IsActive == true && s.StatId == StatId && app_ids_in_group.Contains(s.Clan.AppId ?? -1)).OrderByDescending(s => s.StatValue).ThenBy(s => s.ClanId).ToList();
            ClanStat statForClan = stats.Where(s => s.ClanId == ClanId).FirstOrDefault();
            Clan clan = db.Clan.Where(a => a.ClanId == ClanId).FirstOrDefault();
            ClanController cc = new ClanController(db, authService);
            int totalClans = await cc.getActiveClanCountByAppId((int)clan.AppId);
            if (clan.IsActive == true)
            {
                return new ClanLeaderboardDTO()
                {
                    TotalRankedClans = totalClans,
                    ClanId = ClanId,
                    Index = stats.IndexOf(statForClan),
                    StartIndex = stats.IndexOf(statForClan),
                    StatValue = statForClan.StatValue,
                    ClanName = clan.ClanName,
                    MediusStats = clan.MediusStats
                };
            }
            return StatusCode(400, $"Clan {ClanId} is inactive.");
        }

        [Authorize]
        [HttpGet, Route("getClanLeaderboardIndexCustom")]
        public async Task<dynamic> getClanLeaderboardIndexCustom(int ClanId, int CustomStatId, int AppId)
        {
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == AppId
                                    select a.AppId).ToList();

            List<ClanCustomStat> stats = db.ClanCustomStat.Where(s => s.Clan.IsActive == true && s.StatId == CustomStatId && app_ids_in_group.Contains(s.Clan.AppId ?? -1)).OrderByDescending(s => s.StatValue).ThenBy(s => s.ClanId).ToList();
            ClanCustomStat statForClan = stats.Where(s => s.ClanId == ClanId).FirstOrDefault();
            Clan clan = db.Clan.Where(a => a.ClanId == ClanId).FirstOrDefault();
            ClanController cc = new ClanController(db, authService);
            int totalClans = await cc.getActiveClanCountByAppId((int)clan.AppId);
            if (clan.IsActive == true)
            {
                return new ClanLeaderboardDTO()
                {
                    TotalRankedClans = totalClans,
                    ClanId = ClanId,
                    Index = statForClan == null ? totalClans - 1 : stats.IndexOf(statForClan),
                    StartIndex = statForClan == null ? totalClans - 1 : stats.IndexOf(statForClan),
                    StatValue = statForClan?.StatValue ?? 0,
                    ClanName = clan.ClanName,
                    MediusStats = clan.MediusStats
                };
            }
            return StatusCode(400, $"Clan {ClanId} is inactive.");
        }

        [Authorize]
        [HttpGet, Route("getLeaderboard")]
        public async Task<List<LeaderboardDTO>> getLeaderboard(int StatId, int StartIndex, int Size, int AppId)
        {
            List<AccountStat> stats = db.AccountStat.Where(s => s.Account.IsActive == true && s.StatId == StatId && s.Account.AppId == AppId).OrderByDescending(s => s.StatValue).ThenBy(s => s.AccountId).Skip(StartIndex).Take(Size).ToList();
            AccountController ac = new AccountController(db, authService);

            List<LeaderboardDTO> board = (from s in stats
                                          join a in db.Account
                                            on s.AccountId equals a.AccountId
                                          where a.IsActive == true
                                          select new LeaderboardDTO()
                                          {
                                              TotalRankedAccounts = 0,
                                              StartIndex = StartIndex,
                                              Index = StartIndex + stats.IndexOf(s),
                                              AccountId = s.AccountId,
                                              AccountName = a.AccountName,
                                              StatValue = s.StatValue,
                                              MediusStats = a.MediusStats
                                          }).ToList();

            return board;
        }

        [Authorize]
        [HttpGet, Route("getLeaderboardCustom")]
        public async Task<List<LeaderboardDTO>> getLeaderboardCustom(int CustomStatId, int StartIndex, int Size, int AppId, bool orderAsc = false)
        {
            List<AccountCustomStat> stats = null;
            if (orderAsc)
                stats = db.AccountCustomStat.Where(s => s.Account.IsActive == true && s.StatValue > 0 && s.StatId == CustomStatId && s.Account.AppId == AppId).OrderBy(s => s.StatValue).ThenBy(s => s.AccountId).Skip(StartIndex).Take(Size).ToList();
            else
                stats = db.AccountCustomStat.Where(s => s.Account.IsActive == true && s.StatId == CustomStatId && s.Account.AppId == AppId).OrderByDescending(s => s.StatValue).ThenBy(s => s.AccountId).Skip(StartIndex).Take(Size).ToList();

            AccountController ac = new AccountController(db, authService);

            List<LeaderboardDTO> board = (from s in stats
                                          join a in db.Account
                                            on s.AccountId equals a.AccountId
                                          where a.IsActive == true
                                          select new LeaderboardDTO()
                                          {
                                              TotalRankedAccounts = 0,
                                              StartIndex = StartIndex,
                                              Index = StartIndex + stats.IndexOf(s),
                                              AccountId = s.AccountId,
                                              AccountName = a.AccountName,
                                              StatValue = s.StatValue,
                                              MediusStats = a.MediusStats
                                          }).ToList();

            return board;
        }

        [Authorize]
        [HttpGet, Route("getClanLeaderboard")]
        public async Task<List<ClanLeaderboardDTO>> getClanLeaderboard(int StatId, int StartIndex, int Size, int AppId)
        {
            List<ClanStat> stats = db.ClanStat.Where(s => s.Clan.IsActive == true && s.StatId == StatId && s.Clan.AppId == AppId).OrderByDescending(s => s.StatValue).ThenBy(s => s.ClanId).Skip(StartIndex).Take(Size).ToList();

            List<ClanLeaderboardDTO> board = (from s in stats
                                          join c in db.Clan
                                            on s.ClanId equals c.ClanId
                                            where c.IsActive == true
                                          select new ClanLeaderboardDTO()
                                          {
                                              TotalRankedClans = 0,
                                              StartIndex = StartIndex,
                                              Index = StartIndex + stats.IndexOf(s),
                                              ClanId = s.ClanId,
                                              ClanName = c.ClanName,
                                              StatValue = s.StatValue,
                                              MediusStats = c.MediusStats
                                          }).ToList();

            return board;
        }

        [Authorize]
        [HttpGet, Route("getClanLeaderboardCustom")]
        public async Task<List<ClanLeaderboardDTO>> getClanLeaderboardCustom(int CustomStatId, int StartIndex, int Size, int AppId)
        {
            List<ClanCustomStat> stats = db.ClanCustomStat.Where(s => s.Clan.IsActive == true && s.StatId == CustomStatId && s.Clan.AppId == AppId).OrderByDescending(s => s.StatValue).ThenBy(s => s.ClanId).Skip(StartIndex).Take(Size).ToList();

            List<ClanLeaderboardDTO> board = (from s in stats
                                              join c in db.Clan
                                                on s.ClanId equals c.ClanId
                                              where c.IsActive == true
                                              select new ClanLeaderboardDTO()
                                              {
                                                  TotalRankedClans = 0,
                                                  StartIndex = StartIndex,
                                                  Index = StartIndex + stats.IndexOf(s),
                                                  ClanId = s.ClanId,
                                                  ClanName = c.ClanName,
                                                  StatValue = s.StatValue,
                                                  MediusStats = c.MediusStats
                                              }).ToList();

            return board;
        }

        [Authorize("database")]
        [HttpPost, Route("postStats")]
        public async Task<dynamic> postStats([FromBody] StatPostDTO statData)
        {
            DateTime modifiedDt = DateTime.UtcNow;
            List<AccountStat> playerStats = db.AccountStat.Where(s => s.AccountId == statData.AccountId).OrderBy(s => s.StatId).Select(s => s).ToList();

            foreach (AccountStat pStat in playerStats)
            {
                
                int newValue = statData.stats[pStat.StatId - 1];
                pStat.ModifiedDt = modifiedDt;
                pStat.StatValue = newValue;

                db.AccountStat.Attach(pStat);
                db.Entry(pStat).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            db.SaveChanges();
            return Ok();

        }

        [Authorize("database")]
        [HttpPost, Route("postStatsCustom")]
        public async Task<dynamic> postStatsCustom([FromBody] StatPostDTO statData)
        {
            DateTime modifiedDt = DateTime.UtcNow;
            List<AccountCustomStat> playerStats = db.AccountCustomStat.Where(s => s.AccountId == statData.AccountId).OrderBy(s => s.StatId).Select(s => s).ToList();

            // populate custom stats if not already exists
            for (int i = 0; i < statData.stats.Count; ++i)
            {
                AccountCustomStat existingStat = playerStats.Where(x => x.StatId == (i + 1)).FirstOrDefault();
                if (existingStat == null)
                {
                    AccountCustomStat newStat = new AccountCustomStat()
                    {
                        StatId = i+1,
                        AccountId = statData.AccountId,
                        StatValue = statData.stats[i],
                    };

                    db.AccountCustomStat.Add(newStat);
                }
                else
                {
                    existingStat.StatValue = statData.stats[i];
                    existingStat.ModifiedDt = DateTime.UtcNow;
                }
            }

            db.SaveChanges();
            return Ok();

        }

        [Authorize("database,discord_bot")]
        [HttpPost, Route("resetLeaderboardCustom")]
        public async Task<dynamic> resetLeaderboardCustom([FromBody] ResetStatDTO statData)
        {
            List<AccountCustomStat> playerStats =   (from s in db.AccountCustomStat
                                                     join c in db.Account
                                                       on s.AccountId equals c.AccountId
                                                     where c.AppId == statData.AppId && s.StatId == statData.StatId && s.StatValue > 0
                                                     select s).ToList();
            
            foreach (var playerStat in playerStats)
            {
                playerStat.StatValue = 0;
                playerStat.ModifiedDt = DateTime.UtcNow;
            }

            db.SaveChanges();
            return Ok();

        }

        [Authorize("database")]
        [HttpPost, Route("postClanStats")]
        public async Task<dynamic> postClanStats([FromBody] ClanStatPostDTO statData)
        {
            DateTime modifiedDt = DateTime.UtcNow;
            List<ClanStat> clanStats = db.ClanStat.Where(s => s.ClanId == statData.ClanId).OrderBy(s => s.StatId).Select(s => s).ToList();

            int badStats = clanStats.Where(s => s.StatValue < 0).Count();
            if (badStats > 0)
                return BadRequest("Found a negative stat in array. Can't have those!");

            foreach (ClanStat cStat in clanStats)
            {

                int newValue = statData.stats[cStat.StatId - 1];
                cStat.ModifiedDt = modifiedDt;
                cStat.StatValue = newValue;

                db.ClanStat.Attach(cStat);
                db.Entry(cStat).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            db.SaveChanges();
            return Ok();

        }

        [Authorize("database")]
        [HttpPost, Route("postClanStatsCustom")]
        public async Task<dynamic> postClanStatsCustom([FromBody] ClanStatPostDTO statData)
        {
            DateTime modifiedDt = DateTime.UtcNow;
            List<ClanCustomStat> clanStats = db.ClanCustomStat.Where(s => s.ClanId == statData.ClanId).OrderBy(s => s.StatId).Select(s => s).ToList();

            int badStats = clanStats.Where(s => s.StatValue < 0).Count();
            if (badStats > 0)
                return BadRequest("Found a negative stat in array. Can't have those!");

            // populate custom stats if not already exists
            for (int i = 0; i < statData.stats.Count; ++i)
            {
                ClanCustomStat existingStat = clanStats.Where(x => x.StatId == (i+1)).FirstOrDefault();
                if (existingStat == null)
                {
                    ClanCustomStat newStat = new ClanCustomStat()
                    {
                        StatId = i+1,
                        ClanId = statData.ClanId,
                        StatValue = statData.stats[i],
                    };

                    db.ClanCustomStat.Add(newStat);
                }
                else
                {
                    existingStat.StatValue = statData.stats[i];
                    existingStat.ModifiedDt = DateTime.UtcNow;
                }
            }

            db.SaveChanges();
            return Ok();

        }

    }
}
