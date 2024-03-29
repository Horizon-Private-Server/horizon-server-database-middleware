﻿using System;
using System.Collections.Generic;

namespace Horizon.Database.Entities
{
    public partial class DimAnnouncements
    {
        public int Id { get; set; }
        public string AnnouncementTitle { get; set; }
        public string AnnouncementBody { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime? ModifiedDt { get; set; }
        public DateTime FromDt { get; set; }
        public DateTime? ToDt { get; set; }
        public int? AppId { get; set; }
    }
}
