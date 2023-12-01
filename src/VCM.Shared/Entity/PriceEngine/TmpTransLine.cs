using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Entity.PriceEngine
{
    [Table("TmpTransLine")]
    public class TmpTransLine
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public int ParentLineNo { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public string PluCode { get; set; }
        public string Uom {get;set;}
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal LineAmountInVAT { get; set; }
        public string TaxGroupCode { get; set; }
        public int VATPercent { get; set; }
        public string CupType { get; set; }
        public string Size { get; set; } = String.Empty;
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public string ComboNo { get; set; }
    }
}
