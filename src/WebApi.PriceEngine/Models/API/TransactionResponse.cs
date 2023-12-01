using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VCM.Common.Helpers;

namespace WebApi.PriceEngine.Models.API
{
    public class TransactionResponse
    {
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public bool IsLoyalty { get; set; }
        public string WinCode { get; set; }
        public IEnumerable<ItemResponse> Items { get; set; }
    }
    public class ItemResponse
    {
        public int LineNo { get; set; }
        public int ParentLineNo { get; set; }
        public string Barcode { get; set; }
        public string PluCode
        {
            get
            {
                return BarcodeHelper.GetPluCode(Barcode);
            }
        }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal UnitPrice { get; set; }
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
        public string TaxGroupCode { get; set; }
        public int VatPercent { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal VATAmount
        {
            get
            {
                return MathHelper.CalcVATAmount(Math.Round(UnitPrice * Quantity - DiscountAmount, 0), VatPercent);
            }
        }
        public string CupType { get; set; } = String.Empty;
        public string Size { get; set; } = String.Empty;
        public bool IsTopping { get; set; } = false;
        public bool IsCombo { get; set; } = false;
        public string ComboNo { get; set; } = string.Empty;
        public IEnumerable<DiscountResponse> DiscountEntry { get; set; }
    }
    public class DiscountResponse
    {
        public int LineNo { get; set; }
        public int OrderLineNo { get; set; }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal DiscountAmount { get; set; }
    }

}
