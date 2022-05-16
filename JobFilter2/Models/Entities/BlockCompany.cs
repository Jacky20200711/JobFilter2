using System;
using System.Collections.Generic;

#nullable disable

namespace JobFilter2.Models.Entities
{
    public partial class BlockCompany
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string BlockReason { get; set; }
    }
}
