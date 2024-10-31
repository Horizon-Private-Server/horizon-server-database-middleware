﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Horizon.Database.DTO
{
    public class AccountDTO
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountPassword { get; set; }
        public List<AccountRelationDTO> Friends { get; set; }
        public List<AccountRelationDTO> Ignored { get; set; }
        public List<int> AccountWideStats { get; set; }
        public List<int> AccountCustomWideStats { get; set; }
        public string MediusStats { get; set; }
        public string MachineId { get; set; }
        public bool IsBanned { get; set; }
        public int? AppId { get; set; }
        public int? ClanId { get; set; }
        public string Metadata { get; set; }
        public bool ResetPasswordOnNextLogin { get; set; } = false;
    }

    public class AccountRequestDTO
    {
        public string AccountName { get; set; }
        public string AccountPassword { get; set; }
        public string MachineId { get; set; }
        public string MediusStats { get; set; }
        public int AppId { get; set; }
        public bool PasswordPreHashed { get; set; } = true;
        public bool ResetPasswordOnNextLogin { get; set; } = false;
    }

    public class AccountRelationDTO
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
    }

    public class AccountStatusDTO
    {
        public int AppId { get; set; }
        public int AccountId { get; set; }
        public bool LoggedIn { get; set; }
        public int? GameId { get; set; }
        public int? ChannelId { get; set; }
        public int? WorldId { get; set; }
        public string GameName { get; set; }
    }

    public class AccountJSONModel
    {
        public List<JsonAccountDTO> Accounts { get; set; }
    }

    public class JsonAccountDTO
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountPassword { get; set; }
        public List<int> Friends { get; set; }
        public List<int> Ignored { get; set; }
        public List<int> AccountWideStats { get; set; }
        public string Stats { get; set; }
    }

    public class AccountPasswordRequest
    {
        public int AccountId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }

    public class AccountAndAppIdRequest
    {
        public string AccountName { get; set; }
        public int AppId { get; set; }
    }

    public class AccountChangeNameRequest
    {
        public string AccountName { get; set; }
        public string NewAccountName { get; set; }
        public int AppId { get; set; }
    }

    public class UserDTO
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public List<string> Roles { get; set; }

    }

    public class BanRequestDTO
    {
        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public DateTime ToDt { get; set; }
    }
}
