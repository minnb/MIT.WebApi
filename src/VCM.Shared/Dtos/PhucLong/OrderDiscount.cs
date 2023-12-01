using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderDiscount
    {
        public int LineId { get; set; }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Note { get; set; }
    }
}
