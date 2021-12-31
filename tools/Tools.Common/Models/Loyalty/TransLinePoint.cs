using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Common.Models.Loyalty
{
    public class TransLinePoint
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public string ItemNo { get; set; }
        public decimal LineAmountInclVAT { get; set; }
        public decimal MemberPointsEarn { get; set; }
    }
}
