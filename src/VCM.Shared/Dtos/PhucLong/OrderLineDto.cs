using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VCM.Shared.API.Attribute;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderLineDto
    {
        [Range(1, 99999)]
        public int LineId { get; set; }
        [Range(0, 99999)]
        public int ParentLineId { get; set; }
        [Required]
        [StringLength(18)]
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        [Required]
        public string Uom { get; set; }
        public decimal OldPrice { get; set; }
        public decimal UnitPrice { get; set; }
        [Required]
        [Range(0, 999)]
        [DefaultValue(1)]
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal LineAmount
        {
            get
            {
                return Math.Round(UnitPrice * Quantity - DiscountAmount, 0);
            }
        }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal LineAmountInVAT
        {
            get
            {
                return Math.Round(UnitPrice * Quantity - DiscountAmount, 0);
            }
        }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal LineAmountIncVAT
        {
            get
            {
                return Math.Round(UnitPrice * Quantity - DiscountAmount, 0);
            }
        }
        [Required]
        [StringRange(AllowableValues = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" }, ErrorMessage = "TaxGroupCode không đúng giá trị")]
        public string TaxGroupCode { get; set; }
        [Required]
        public int VatPercent { get; set; }
        public string Note { get; set; } = String.Empty;
        [StringRange(AllowableValues = new[] {"", "PAPER", "PLASTIC" }, ErrorMessage = "Loại Ly không đúng 'PAPER' or 'PLASTIC'")]
        public string CupType { get; set; } = String.Empty;
        [StringRange(AllowableValues = new[] { "", "H", "M", "L", "S" }, ErrorMessage = "Size sản phẩm không đúng H, M, L, S")]
        public string Size { get; set; } = String.Empty;
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public string ComboNo { get; set; }
        public int ComboId { get; set; }
        public string ArticleType { get; set; } = String.Empty;
        public string Barcode { get; set; } = String.Empty;
        public bool IsLoyalty { get; set; }
        public List<OrderLoyalty> Loyalty { get; set; }
        public List<OrderLineOptionDto> OptionEntry { get; set; }
        public List<OrderDiscount> DiscountEntry { get; set; }
    }
    public class RspOrderLineDto
    {
        public int LineId { get; set; }
        public int ParentLineId { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal OldPrice { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal LineAmount { get; set; }
        public string TaxGroupCode { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal VatPercent { get; set; }
        public string Note { get; set; } = String.Empty;
        public bool IsLoyalty { get; set; }
        public string CupType { get; set; } = String.Empty;
        public string Size { get; set; } = String.Empty;
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public string ComboNo { get; set; }
        public string ArticleType { get; set; } = String.Empty;
        public string Barcode { get; set; } = String.Empty;
        public List<RspOrderLineOptionDto> OptionEntry { get; set; }
        public List<OrderDiscount> DiscountEntry { get; set; }
        public List<OrderLoyalty> Loyalty { get; set; }
        public string DiscountType { get; set; }
    }
}
