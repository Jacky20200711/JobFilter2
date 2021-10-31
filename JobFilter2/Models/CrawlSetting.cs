using System;
using System.Collections.Generic;

#nullable disable

namespace JobFilter2.Models
{
    public partial class CrawlSetting
    {
        public int Id { get; set; }
        public string TargetUrl { get; set; }
        public int MinSalary { get; set; }
        public string Seniority { get; set; }
        public string Remark { get; set; }
    }
}
