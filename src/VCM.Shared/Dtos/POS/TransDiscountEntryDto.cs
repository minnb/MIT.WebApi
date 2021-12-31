using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Shared.Dtos.POS
{
    public class TransDiscountEntryDto
    {
        public int LineNo { get; set; }
        public int ParentLineNo { get; set; }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Qty { get; set; }
        public string Note { get; set; }
    }
}
