﻿using System;
using System.Collections.Generic;

namespace Horizon.Database.Entities
{
    public partial class AccountCustomStat
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int StatId { get; set; }
        public int StatValue { get; set; }
        public DateTime? ModifiedDt { get; set; }

        public virtual Account Account { get; set; }
        public virtual DimCustomStats Stat { get; set; }
    }
}
