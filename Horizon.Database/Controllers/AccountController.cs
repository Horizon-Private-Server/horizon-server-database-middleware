﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Horizon.Database.DTO;
using Horizon.Database.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Horizon.Database.Models;
using Horizon.Database.Services;
using Horizon.Database.Helpers;

namespace Horizon.Database.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        private IAuthService authService;
        public AccountController(Ratchet_DeadlockedContext _db, IAuthService _authService)
        {
            db = _db;
            authService = _authService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticationRequest model)
        {
            var response = authService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [Authorize]
        [HttpGet, Route("getActiveAccountCountByAppId")]
        public async Task<int> getActiveAccountCountByAppId(int AppId)
        {
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == AppId
                                    select a.AppId).ToList();

            int accountCount = (from a in db.Account
                                where app_ids_in_group.Contains(a.AppId ?? -1)
                                && a.IsActive == true
                                select a).Count();
            return accountCount;
        }

        [Authorize("database")]
        [HttpGet, Route("getAccount")]
        public async Task<dynamic> getAccount(int AccountId)
        {
            DateTime now = DateTime.UtcNow;
            Account existingAccount = db.Account.Include(a => a.ClanMember).Where(a => a.AccountId == AccountId).FirstOrDefault();
            //Account existingAccount = db.Account//.Include(a => a.AccountFriend)
            //                                    //.Include(a => a.AccountIgnored)
            //                                    .Include(a => a.AccountStat)
            //                                    .Where(a => a.AccountId == AccountId)
            //                                    .FirstOrDefault();



            if (existingAccount == null)
                return NotFound();

            var existingBan = (from b in db.Banned where b.AccountId == existingAccount.AccountId && b.FromDt <= now && (b.ToDt == null || b.ToDt > now) select b).FirstOrDefault();
            bool accountBanned = existingBan != null ? true : false;
            bool macBanned = await getMacIsBanned(existingAccount.MachineId);
            
            var accountList = db.Account.ToList();

            AccountDTO account2 = (from a in db.Account
                                   where a.AccountId == AccountId
                                   select new AccountDTO()
                                   {
                                       AccountId = a.AccountId,
                                       AccountName = a.AccountName,
                                       AccountPassword = a.AccountPassword,
                                       AccountWideStats = a.AccountStat.OrderBy(s => s.StatId).Select(s => s.StatValue).ToList(),
                                       AccountCustomWideStats = a.AccountCustomStat.OrderBy(s => s.StatId).Select(s => s.StatValue).ToList(),
                                       Friends = new List<AccountRelationDTO>(),
                                       Ignored = new List<AccountRelationDTO>(),
                                       Metadata = existingAccount.Metadata,
                                       MediusStats = existingAccount.MediusStats,
                                       MachineId = existingAccount.MachineId,
                                       IsBanned = accountBanned || macBanned,
                                       AppId = existingAccount.AppId,
                                       ResetPasswordOnNextLogin = a.ResetPasswordOnNextLogin
                                   }).FirstOrDefault();
            List<int> friendIds = db.AccountFriend.Where(a => a.AccountId == AccountId).Select(a => a.FriendAccountId).ToList();
            List<int> ignoredIds = db.AccountIgnored.Where(a => a.AccountId == AccountId).Select(a => a.IgnoredAccountId).ToList();

            account2.ClanId = existingAccount.ClanMember.Where(cm => cm.IsActive == true).FirstOrDefault()?.ClanId;

            foreach (int friendId in friendIds)
            {
                AccountRelationDTO friendDTO = new AccountRelationDTO()
                {
                    AccountId = friendId,
                    AccountName = accountList.Where(a => a.AccountId == friendId).Select(a => a.AccountName).FirstOrDefault()
                };
                account2.Friends.Add(friendDTO);
            }
            foreach (int ignoredId in ignoredIds)
            {
                AccountRelationDTO friendDTO = new AccountRelationDTO()
                {
                    AccountId = ignoredId,
                    AccountName = accountList.Where(a => a.AccountId == ignoredId).Select(a => a.AccountName).FirstOrDefault()
                };
                account2.Ignored.Add(friendDTO);
            }

            return account2;
        }


        [Authorize("discord_bot")]
        [HttpGet, Route("getAccountBasic")]
        public async Task<dynamic> getAccountBasic(int AccountId)
        {
            DateTime now = DateTime.UtcNow;
            Account existingAccount = db.Account.Include(a => a.ClanMember).Where(a => a.AccountId == AccountId).FirstOrDefault();

            if (existingAccount == null)
                return NotFound();

            var existingBan = (from b in db.Banned where b.AccountId == existingAccount.AccountId && b.FromDt <= now && (b.ToDt == null || b.ToDt > now) select b).FirstOrDefault();

            AccountDTO account2 = (from a in db.Account
                                   where a.AccountId == AccountId
                                   select new AccountDTO()
                                   {
                                       AccountId = a.AccountId,
                                       AccountName = a.AccountName,
                                       AccountPassword = "",
                                       AccountWideStats = a.AccountStat.OrderBy(s => s.StatId).Select(s => s.StatValue).ToList(),
                                       AccountCustomWideStats = a.AccountCustomStat.OrderBy(s => s.StatId).Select(s => s.StatValue).ToList(),
                                       Friends = new List<AccountRelationDTO>(),
                                       Ignored = new List<AccountRelationDTO>(),
                                       Metadata = existingAccount.Metadata,
                                       MediusStats = existingAccount.MediusStats,
                                       MachineId = "",
                                       IsBanned = existingBan != null ? true : false,
                                       AppId = existingAccount.AppId,
                                       ResetPasswordOnNextLogin = false
                                   }).FirstOrDefault();

            account2.ClanId = existingAccount.ClanMember.Where(cm => cm.IsActive == true).FirstOrDefault()?.ClanId;

            return account2;
        }

        [Authorize]
        [HttpGet, Route("getAccountMetadata")]
        public async Task<string> getAccountMetadata(int AccountId)
        {
            string metadata = (from a in db.Account
                           where a.AccountId == AccountId
                           select a.Metadata).FirstOrDefault();
            return metadata;
        }

        [Authorize("database")]
        [HttpPost, Route("createAccount")]
        public async Task<dynamic> createAccount([FromBody] AccountRequestDTO request)
        {
            DateTime now = DateTime.UtcNow;
            Account existingAccount = db.Account.Where(a => a.AccountName == request.AccountName).FirstOrDefault();
            if (existingAccount == null || existingAccount.IsActive == false)
            {
                if (existingAccount == null)
                {
                    Account acc = new Account()
                    {
                        AccountName = request.AccountName,
                        AccountPassword = request.PasswordPreHashed ? request.AccountPassword : Crypto.ComputeSHA256(request.AccountPassword),
                        CreateDt = now,
                        LastSignInDt = now,
                        MachineId = request.MachineId,
                        MediusStats = request.MediusStats,
                        AppId = request.AppId,
                        ResetPasswordOnNextLogin = request.ResetPasswordOnNextLogin,
                    };


                    db.Account.Add(acc);
                    db.SaveChanges();


                    List<AccountStat> newStats = (from ds in db.DimStats
                                                  select new AccountStat()
                                                  {
                                                      AccountId = acc.AccountId,
                                                      StatId = ds.StatId,
                                                      StatValue = ds.DefaultValue
                                                  }).ToList();
                    db.AccountStat.AddRange(newStats);

                    List<AccountCustomStat> newCustomStats = (from ds in db.DimCustomStats
                                                              where (ds.AppId == 0 || ds.AppId == acc.AppId)
                                                              select new AccountCustomStat()
                                                              {
                                                                  AccountId = acc.AccountId,
                                                                  StatId = ds.StatId,
                                                                  StatValue = ds.DefaultValue
                                                              }).ToList();
                    db.AccountCustomStat.AddRange(newCustomStats);

                    AccountStatus newStatusData = new AccountStatus()
                    {
                        AppId = acc.AppId ?? 0,
                        AccountId = acc.AccountId,
                        GameName = null,
                        LoggedIn = false,
                        GameId = null,
                        ChannelId = null,
                        WorldId = null,
                        DatabaseUser = HttpContext.GetUsernameOrDefault()
                    };
                    db.AccountStatus.Add(newStatusData);

                    db.SaveChanges();
                    return await getAccount(acc.AccountId);
                } else
                {
                    existingAccount.IsActive = true;
                    existingAccount.AccountPassword = request.AccountPassword;
                    existingAccount.ModifiedDt = now;
                    existingAccount.MediusStats = request.MediusStats;
                    existingAccount.AppId = request.AppId;
                    existingAccount.MachineId = request.MachineId;
                    existingAccount.LastSignInDt = now;
                    existingAccount.ResetPasswordOnNextLogin = false;
                    db.Account.Attach(existingAccount);
                    db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    // delete stats
                    List<AccountStat> existingStats = db.AccountStat.Where(s => s.AccountId == existingAccount.AccountId).ToList();
                    db.RemoveRange(existingStats);

                    // delete custom stats
                    List<AccountCustomStat> existingCustomStats = db.AccountCustomStat.Where(s => s.AccountId == existingAccount.AccountId).ToList();
                    db.RemoveRange(existingCustomStats);

                    // delete friends
                    List<AccountFriend> existingFriends = db.AccountFriend.Where(s => s.AccountId == existingAccount.AccountId).ToList();
                    db.RemoveRange(existingFriends);

                    // delete buddies
                    List<AccountIgnored> existingIgnores = db.AccountIgnored.Where(ai => ai.AccountId == existingAccount.AccountId).ToList();
                    db.RemoveRange(existingIgnores);

                    // add fresh stats
                    List<AccountStat> newStats = (from ds in db.DimStats
                                                  select new AccountStat()
                                                  {
                                                      AccountId = existingAccount.AccountId,
                                                      StatId = ds.StatId,
                                                      StatValue = ds.DefaultValue
                                                  }).ToList();
                    db.AccountStat.AddRange(newStats);

                    // add fresh custom stats
                    List<AccountCustomStat> newCustomStats = (from ds in db.DimCustomStats
                                                              where (ds.AppId == 0 || ds.AppId == existingAccount.AppId)
                                                              select new AccountCustomStat()
                                                              {
                                                                  AccountId = existingAccount.AccountId,
                                                                  StatId = ds.StatId,
                                                                  StatValue = ds.DefaultValue
                                                              }).ToList();
                    db.AccountCustomStat.AddRange(newCustomStats);

                    db.SaveChanges();
                    return await getAccount(existingAccount.AccountId);
                }

            } else
            {
                return StatusCode(403, $"Account {request.AccountName} already exists.");
            }
        }

        [Authorize("database")]
        [HttpGet, Route("deleteAccount")]
        public async Task<dynamic> deleteAccount(string AccountName, int AppId)
        {
            DateTime now = DateTime.UtcNow;
            Account existingAccount = db.Account.Where(a => a.AccountName == AccountName && a.AppId == AppId).FirstOrDefault();
            if(existingAccount == null || existingAccount.IsActive == false)
            {
                return StatusCode(403, "Cannot delete an account that doesn't exist.");
            }

            existingAccount.IsActive = false;
            existingAccount.ModifiedDt = now;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            AccountDTO otherData = await getAccount(existingAccount.AccountId);

            List<AccountStat> existingStats = db.AccountStat.Where(s => s.AccountId == existingAccount.AccountId).ToList();
            db.RemoveRange(existingStats);

            List<AccountCustomStat> existingCustomStats = db.AccountCustomStat.Where(s => s.AccountId == existingAccount.AccountId).ToList();
            db.RemoveRange(existingCustomStats);

            List<AccountFriend> existingFriends = db.AccountFriend.Where(s => s.AccountId == existingAccount.AccountId).ToList();
            db.RemoveRange(existingFriends);

            List<AccountIgnored> existingIgnores = db.AccountIgnored.Where(ai => ai.AccountId == existingAccount.AccountId).ToList();
            db.RemoveRange(existingIgnores);

            db.SaveChanges();
            return Ok("Account Deleted");
        }

        [Authorize]
        [HttpGet, Route("searchAccountByName")]
        public async Task<dynamic> searchAccountByName(string AccountName, int AppId)
        {
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == AppId
                                    select a.AppId).ToList();

            Account existingAccount = db.Account.Where(a => app_ids_in_group.Contains(a.AppId ?? -1) && a.AccountName == AccountName && a.IsActive == true).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            return await getAccount(existingAccount.AccountId);
        }

        [Authorize("database")]
        [HttpPost, Route("postMachineId")]
        public async Task<dynamic> postMachineId([FromBody] string MachineId, int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            existingAccount.MachineId = MachineId;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }

        [Authorize("database")]
        [HttpPost, Route("postMediusStats")]
        public async Task<dynamic> postMediusStats([FromBody] string StatsString, int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            existingAccount.MediusStats = StatsString;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }
        [Authorize("database")]
        [HttpPost, Route("postAccountSignInDate")]
        public async Task<dynamic> postAccountSignInDate([FromBody] DateTime SignInDt, int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();

            if (existingAccount == null)
                return NotFound();

            existingAccount.LastSignInDt = SignInDt;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();

            return Ok();
        }
        [Authorize("database")]
        [HttpPost, Route("postAccountIp")]
        public async Task<dynamic> postAccountIp([FromBody] string IpAddress, int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            existingAccount.LastSignInIp = IpAddress;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }

        [Authorize("database,moderator")]
        [HttpPost, Route("postAccountMetadata")]
        public async Task<dynamic> postAccountMetadata([FromBody] string Metadata, int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            existingAccount.Metadata = Metadata;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }

        [Authorize]
        [HttpGet, Route("getAccountStatus")]
        public async Task<dynamic> getAccountStatus(int AccountId)
        {
            AccountStatus existingData = db.AccountStatus.Where(acs => acs.AccountId == AccountId).FirstOrDefault();
            if (existingData == null)
                return NotFound();

            return existingData;
        }

        [Authorize("database")]
        [HttpPost, Route("postAccountStatusUpdates")]
        public async Task<dynamic> postAccountStatusUpdates([FromBody] AccountStatusDTO StatusData)
        {
            AccountStatus existingData = db.AccountStatus.Where(acs => acs.AccountId == StatusData.AccountId).FirstOrDefault();
            if (existingData != null)
            {
                existingData.LoggedIn = StatusData.LoggedIn;
                existingData.GameId = StatusData.GameId;
                existingData.ChannelId = StatusData.ChannelId;
                existingData.WorldId = StatusData.WorldId;
                existingData.GameName = StatusData.GameName;
                existingData.AppId = StatusData.AppId;
                existingData.DatabaseUser = HttpContext.GetUsernameOrDefault();
                db.AccountStatus.Attach(existingData);
                db.Entry(existingData).State = EntityState.Modified;
            }
            db.SaveChanges();

            return await getAccountStatus(StatusData.AccountId);
        }

        [Authorize("database")]
        [HttpPost, Route("clearAccountStatuses")]
        public async Task<dynamic> clearAccountStatuses()
        {
            await db.AccountStatus.ForEachAsync(a =>
            {
                a.GameId = null;
                a.LoggedIn = false;
                a.WorldId = null;
                a.ChannelId = null;
                a.GameName = null;
            });

            db.SaveChanges();

            return Ok();
        }

        [Authorize("discord_bot")]
        [HttpGet, Route("getOnlineAccounts")]
        public async Task<dynamic> getOnlineAccounts()
        {
            var results = db.AccountStatus
                .Where(acs => acs.LoggedIn)
                .Join(db.Account,
                    acs => acs.AccountId,
                    a => a.AccountId,
                    (acs, a) => new { acs, a })
                .Where(joined => joined.a.MachineId != null)
                .Select(joined => new
                {
                    joined.acs.AppId,
                    joined.acs.AccountId,
                    joined.a.AccountName,
                    joined.acs.WorldId,
                    joined.acs.GameId,
                    joined.acs.GameName,
                    joined.acs.ChannelId,
                    joined.acs.DatabaseUser,
                    joined.a.Metadata
                });

            return results;
        }

        [Authorize("database")]
        [HttpPost, Route("changeAccountPassword")]
        public async Task<dynamic> changeAccountPassword([FromBody] AccountPasswordRequest PasswordRequest)
        {
            Account existingAccount = db.Account.Where(acs => acs.AccountId == PasswordRequest.AccountId).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            if (!existingAccount.ResetPasswordOnNextLogin && Crypto.ComputeSHA256(PasswordRequest.OldPassword) != existingAccount.AccountPassword)
                return StatusCode(401, "The password you provided is incorrect.");

            if (PasswordRequest.NewPassword != PasswordRequest.ConfirmNewPassword)
                return StatusCode(400, "The new and confirmation passwords do not match each other. Please try again.");

            existingAccount.ResetPasswordOnNextLogin = false;
            existingAccount.AccountPassword = Crypto.ComputeSHA256(PasswordRequest.NewPassword);
            existingAccount.ModifiedDt = DateTime.UtcNow;

            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = EntityState.Modified;
            db.SaveChanges();

            return Ok("Password Updated");

        }

        [Authorize("database,moderator")]
        [HttpPost, Route("resetAccountPassword")]
        public async Task<dynamic> resetAccountPassword([FromBody] AccountAndAppIdRequest PasswordRequest)
        {
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == PasswordRequest.AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == PasswordRequest.AppId
                                    select a.AppId).ToList();

            Account existingAccount = db.Account.Where(a => app_ids_in_group.Contains(a.AppId ?? -1) && a.AccountName == PasswordRequest.AccountName && a.IsActive == true).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            existingAccount.ResetPasswordOnNextLogin = true;
            existingAccount.ModifiedDt = DateTime.UtcNow;

            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = EntityState.Modified;
            db.SaveChanges();

            return Ok("Password Reset");
        }

        [Authorize("moderator")]
        [HttpPost, Route("changeAccountName")]
        public async Task<dynamic> changeAccountName([FromBody] AccountChangeNameRequest ChangeNameRequest)
        {
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == ChangeNameRequest.AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == ChangeNameRequest.AppId
                                    select a.AppId).ToList();

            Account existingAccount = db.Account.Where(a => app_ids_in_group.Contains(a.AppId ?? -1) && a.AccountName == ChangeNameRequest.AccountName && a.IsActive == true).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            app_id_group = (from a in db.DimAppIds
                                where a.AppId == ChangeNameRequest.AppId
                                select a.GroupId).FirstOrDefault();

            app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == ChangeNameRequest.AppId
                                    select a.AppId).ToList();

            Account newAccount = db.Account.Where(a => app_ids_in_group.Contains(a.AppId ?? -1) && a.AccountName == ChangeNameRequest.NewAccountName && a.IsActive == true).FirstOrDefault();
            if (newAccount != null)
                return StatusCode(403, "The account name already exists.");

            // Check text filters to make sure new name passes 
            var settings = await (from s in db.ServerSettings
                            where s.AppId == ChangeNameRequest.AppId
                            select new { s.Name, s.Value }).ToDictionaryAsync(x => x.Name, x => x.Value);

            string regex = "";
            if (settings.ContainsKey("TextFilterAccountName")) 
            {
                regex = settings["TextFilterAccountName"];
            }
            // Use normal text filter since no account name filter exists
            else if (settings.ContainsKey("TextFilterDefault")) 
            {
                regex = settings["TextFilterDefault"];
            }

            // Check if name passes regex
            if (!Utils.PassTextFilter(ChangeNameRequest.NewAccountName, regex)) {
                return StatusCode(403, "Did not pass text filter!");
            }

            existingAccount.AccountName = ChangeNameRequest.NewAccountName;
            existingAccount.ModifiedDt = DateTime.UtcNow;

            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = EntityState.Modified;
            db.SaveChanges();

            return Ok("Account Name Changed.");
        }

        [Authorize("database")]
        [HttpPost, Route("getIpIsBanned")]
        public async Task<bool> getIpIsBanned([FromBody] string IpAddress)
        {
            DateTime now = DateTime.UtcNow;
            BannedIp ban = (from b in db.BannedIp
                            where b.IpAddress == IpAddress
                            && b.FromDt <= now
                            && (b.ToDt == null || b.ToDt > now)
                            select b).FirstOrDefault();
            return ban != null ? true : false;
        }

        [Authorize("database")]
        [HttpPost, Route("getMacIsBanned")]
        public async Task<bool> getMacIsBanned([FromBody] string MacAddress)
        {
            DateTime now = DateTime.UtcNow;
            BannedMac ban = (from b in db.BannedMac
                             where b.MacAddress == MacAddress
                            && b.FromDt <= now
                            && (b.ToDt == null || b.ToDt > now)
                            select b).FirstOrDefault();
            return ban != null ? true : false;
        }

        
        [Authorize("database")]
        [HttpGet, Route("checkAccountIsBanned")]
        public async Task<bool> checkAccountIsBanned(string AccountName, int AppId)
        {
            DateTime now = DateTime.UtcNow;

            // Get machine id and IP
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == AppId
                                    select a.AppId).ToList();

            var existingAccount = db.Account
                .Where(a => app_ids_in_group.Contains(a.AppId ?? -1) && a.AccountName == AccountName && a.IsActive == true)
                .Select(a => new 
                {
                    a.AccountId,
                    a.LastSignInIp,
                    a.MachineId
                })
                .FirstOrDefault();

            if (existingAccount == null) {
                return false;
            }

            // Check for MAC Ban
            bool macBanned = await getMacIsBanned(existingAccount.MachineId);
            if (macBanned)
                return true;

            // Check for IP Ban
            if (existingAccount.LastSignInIp != null) {
                bool ipBanned = await getIpIsBanned(existingAccount.LastSignInIp);
                if (ipBanned) 
                    return true;
            }

            // Check for Account Ban
            var existingBan = (from b in db.Banned where b.AccountId == existingAccount.AccountId && b.FromDt <= now && (b.ToDt == null || b.ToDt > now) select b).FirstOrDefault();
            bool accountBanned = existingBan != null ? true : false;
            if (accountBanned)
                return true;

            return false;
        }

        [Authorize("database")]
        [HttpGet, Route("getAccountNameMacIsBanned")]
        public async Task<bool> getAccountNameMacIsBanned(string AccountName, int AppId)
        {
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == AppId
                                    select a.AppId).ToList();

            Account existingAccount = db.Account.Where(a => app_ids_in_group.Contains(a.AppId ?? -1) && a.AccountName == AccountName && a.IsActive == true).FirstOrDefault();
            if (existingAccount == null)
                return false;

            bool result = await getMacIsBanned(existingAccount.MachineId);

            return result;
        }

        [Authorize("database")]
        [HttpPost, Route("banIp")]
        public async Task<dynamic> banIp([FromBody] BanRequestDTO request)
        {
            DateTime now = DateTime.UtcNow;
            BannedIp newBan = new BannedIp()
            {
                IpAddress = request.IpAddress,
                FromDt = now,
                ToDt = request.ToDt
            };
            db.BannedIp.Add(newBan);
            db.SaveChanges();
            return Ok("Ip Banned");
        }

        [Authorize("database")]
        [HttpPost, Route("banMac")]
        public async Task<dynamic> banMac([FromBody] BanRequestDTO request)
        {
            DateTime now = DateTime.UtcNow;
            BannedMac newBan = new BannedMac()
            {
                MacAddress = request.MacAddress,
                FromDt = now,
                ToDt = request.ToDt
            };
            db.BannedMac.Add(newBan);
            db.SaveChanges();
            return Ok("Mac Banned");
        }

        [Authorize("moderator")]
        [HttpPost, Route("banAccount")]
        public async Task<dynamic> banAccount([FromBody] AccountAndAppIdRequest request) 
        {
            DateTime now = DateTime.UtcNow;
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == request.AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == request.AppId
                                    select a.AppId).ToList();

            Account existingAccount = db.Account.Where(a => app_ids_in_group.Contains(a.AppId ?? -1) && a.AccountName == request.AccountName && a.IsActive == true).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            Banned newBan = new Banned()
            {
                AccountId = existingAccount.AccountId,
                FromDt = now
            };
            db.Banned.Add(newBan);
            db.SaveChanges();
            return Ok("Account Banned");

        }

        [Authorize("moderator")]
        [HttpPost, Route("banIpByAccountName")]
        public async Task<dynamic> banIpByAccountName([FromBody] AccountAndAppIdRequest request) 
        {
            DateTime now = DateTime.UtcNow;
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == request.AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == request.AppId
                                    select a.AppId).ToList();

            Account existingAccount = db.Account.Where(a => app_ids_in_group.Contains(a.AppId ?? -1) && a.AccountName == request.AccountName && a.IsActive == true).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            if (existingAccount.LastSignInIp == null)
                return StatusCode(403, "No IP for that account!");

            BannedIp newBan = new BannedIp()
            {
                IpAddress = existingAccount.LastSignInIp,
                FromDt = now
            };
            db.BannedIp.Add(newBan);
            db.SaveChanges();
            return Ok("Ip Banned");
        }

        [Authorize("moderator")]
        [HttpPost, Route("banMacByAccountName")]
        public async Task<dynamic> banMacByAccountName([FromBody] AccountAndAppIdRequest request) 
        {
            DateTime now = DateTime.UtcNow;
            var app_id_group = (from a in db.DimAppIds
                                where a.AppId == request.AppId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == request.AppId
                                    select a.AppId).ToList();

            Account existingAccount = db.Account.Where(a => app_ids_in_group.Contains(a.AppId ?? -1) && a.AccountName == request.AccountName && a.IsActive == true).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            if (existingAccount.MachineId == null)
                return StatusCode(403, "No MAC for that account!");

            BannedMac newBan = new BannedMac()
            {
                MacAddress = existingAccount.MachineId,
                FromDt = now
            };
            db.BannedMac.Add(newBan);
            db.SaveChanges();
            return Ok("Mac Banned");
        }


    }
}
