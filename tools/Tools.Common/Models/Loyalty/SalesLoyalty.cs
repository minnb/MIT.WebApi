using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Common.Models.Loyalty
{
    public class SalesLoyalty
    {
        public string OrderNo { get; set; }
        public string RefNo { get; set; }
        public string PosNo { get; set; }
        public string Phone { get; set; }
        public decimal AmountInclVAT { get; set; }
    }
}
