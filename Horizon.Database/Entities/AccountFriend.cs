﻿using System;
using System.Collections.Generic;

namespace Horizon.Database.Entities
{
    public partial class AccountFriend
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int FriendAccountId { get; set; }
        public DateTime CreateDt { get; set; }

        public virtual Account Account { get; set; }
    }
}
