using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Dtos.POS
{
    public class TransDiscountEntryDto
    {
        public int LineNo { get; set; }
        public int ParentLineNo { get; set; }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal DiscountAmount { get; set; }
        public decimal Qty { get; set; }
        public string Note { get; set; }
    }
}
