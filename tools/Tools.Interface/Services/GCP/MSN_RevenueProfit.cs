using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Services.GCP
{
    public class MSN_RevenueProfit
    {
        public long Id { get; set; }
        public string Year { get; set; }
        public string BU { get; set; }
        public string Revenue { get; set; }
        public string Profit { get; set; }
        public string Hrcost { get; set; }
        public int? ProcessingMonth { get; set; }
        public int? ProcessingYear { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FileName { get; set; }
    }
}
