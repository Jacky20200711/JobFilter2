﻿using System;
using System.Collections.Generic;

#nullable disable

namespace JobFilter2.Models.Entities
{
    public partial class CrawlSetting
    {
        public int Id { get; set; }
        public string TargetUrl { get; set; }
        public int MinSalary { get; set; }
        public string Seniority { get; set; }
        public string Remark { get; set; }
        public string ExcludeWords { get; set; }
        public string HasSalary { get; set; }
        public int MaxSalary { get; set; }
    }
}
