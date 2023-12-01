using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Entity.PriceEngine
{
    [Table("TmpTransDiscount")]
    public class TmpTransDiscount
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public int OrderLineNo { get; set; }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        public string ItemNo { get; set; }
        public string BarcodeNo { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal DiscountAmount { get; set; }
    }
}
