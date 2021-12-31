using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderDiscount
    {
        public int LineId { get; set; }
        public string PromotionNo { get; set; }
        public string PromotionType { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }
}
