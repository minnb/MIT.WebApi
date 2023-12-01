using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;

namespace WebApi.PriceEngine.Models.API
{
    public class SalesPriceResponse
    {
        public string ItemNo { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; } = 1;
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
        public decimal LineAmountIncVAT 
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
        public string TaxGroupCode { get; set; }
        public int VatPercent { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal VatAmount 
        {
            get
            {
                return MathHelper.CalcVATAmount(Math.Round(UnitPrice * Quantity - DiscountAmount, 0), VatPercent);
            }
        }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        public string DiscountType { get; set; }
        public string PluCode
        {
            get
            {
                return BarcodeHelper.GetPluCode(Barcode);
            }
        }
    }
}
