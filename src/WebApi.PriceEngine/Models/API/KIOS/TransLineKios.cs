using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VCM.Common.Helpers;
using VCM.Shared.API.Attribute;

namespace WebApi.PriceEngine.Models.API.KIOS
{
    public class TransLineKios
    {
        public string OrderNo { get; set; }
        [Range(1, 99999)]
        public int LineId { get; set; }
        [Range(1, 99)]
        public int LineType { get; set; }
        [Range(0, 99999)]
        public int ParentLineId { get; set; }
        [Required]
        [StringLength(18)]
        public string ItemNo { get; set; }
        public string Barcode { get; set; } = String.Empty;
        public string ItemName { get; set; }
        [Required]
        public string Uom { get; set; }
        public decimal UnitPrice { get; set; }
        [Required]
        [Range(0, 999)]
        [DefaultValue(1)]
        public decimal Quantity { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineAmountIncVAT
        {
            get
            {
                return Math.Round(UnitPrice * Quantity - DiscountAmount, 0);
            }
        }
        public decimal LineAmountInVAT
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
        public decimal VatAmount { get; set; }
        [StringRange(AllowableValues = new[] { "", "PAPER", "PLASTIC" }, ErrorMessage = "Loại Ly không đúng 'PAPER' or 'PLASTIC'")]
        public string CupType { get; set; } = String.Empty;
        [StringRange(AllowableValues = new[] { "", "H", "M", "L", "S" }, ErrorMessage = "Size sản phẩm không đúng H, M, L, S")]
        public string Size { get; set; } = String.Empty;
        public bool IsTopping { get; set; }
        public string Note { get; set; } = String.Empty;
        public decimal MemberPointsEarn { get; set; }
        public decimal MemberPointsRedeem { get; set; }
        public List<TransOptionEntryKios> OptionEntry { get; set; }
        public List<TransDiscountEntryKios> DiscountEntry { get; set; }
    }
    public class TransDiscountEntryKios
    {
        public string OrderNo { get; set; }
        public int LineId { get; set; }
        public int ParentLineId { get; set; }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Note { get; set; }
    }
    public class TransOptionEntryKios
    {
        public string OrderNo { get; set; }
        public int LineId { get; set; }
        public int ParentLineId { get; set; }
        public string Type { get; set; }
        public string ItemNo { get; set; }
        public string OptionType { get; set; }
        public string OptionName { get; set; }
        public string Uom { get; set; }
        public decimal Qty { get; set; }
        public string Size { get; set; }
    }
}
