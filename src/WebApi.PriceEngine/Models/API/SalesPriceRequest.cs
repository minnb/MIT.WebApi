using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;

namespace WebApi.PriceEngine.Models.API
{
    public class CheckSalesPriceRequest
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        [DefaultValue("WCM")]
        public string AppCode { get; set; } = "WCM";

        [Required]
        [StringLength(4, MinimumLength = 4)]
        [DefaultValue("2018")]
        public string StoreNo { get; set; } = "2018";

        [Required]
        [StringLength(19, MinimumLength = 8, ErrorMessage = "Barcode phải là một chuỗi có độ dài tối thiểu là 8 và độ dài tối đa là 19")]
        [RegularExpression(@"^\$?\d+(\.(\d{2}))?$", ErrorMessage="Barcode chỉ bao gồm các số 0-9")]
        [DefaultValue("2050000311444")]
        public string Barcode { get; set; } = "2050000311444";

        [Required]
        [Range(0.0001, 999.99, ErrorMessage = "Số lượng bán chỉ từ 0.001 tới 999.999")]
        [DefaultValue(1)]
        public decimal Quantity 
        {
            get
            {
                return BarcodeHelper.GetQtyBarcode(Barcode);
            }
        } 
        public string PluCode
        {
            get
            {
                return BarcodeHelper.GetPluCode(Barcode);
            }
        }
    }
    public class BarcodeSalesPriceRequest
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        [DefaultValue("WCM")]
        public string AppCode { get; set; } = "WCM";

        [Required]
        [StringLength(4, MinimumLength = 4)]
        [DefaultValue("2018")]
        public string StoreNo { get; set; } = "2018";
        [Required]
        public IEnumerable<SKURequest> SKUs { get; set; }
    }
    public class ItemSalesPriceRequest
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        [DefaultValue("WCM")]
        public string AppCode { get; set; } = "WCM";

        [Required]
        [StringLength(4, MinimumLength = 4)]
        [DefaultValue("2018")]
        public string StoreNo { get; set; } = "2018";
        [Required]
        public IEnumerable<ItemsRequest> SKUs { get; set; }
    }
    public class SKURequest
    {
        [Required]
        [StringLength(18, MinimumLength = 8)]
        [DefaultValue("2050000311444")]
        public string Barcode { get; set; } = "2050000311444";

        [Required]
        [Range(0.0001, 999.99)]
        [DefaultValue(1)]
        public decimal Quantity { get; set; } = 1;
        public string PluCode
        {
            get
            {
                return _isFreshFoodBarcode && Barcode.Length == 13 ? Barcode.Substring(0, 7) + "000000" : Barcode;
            }
        }
        private bool _isFreshFoodBarcode
        {
            get
            {
                return (Barcode.Substring(0, 2) == "26"
                   || Barcode.Substring(0, 2) == "11") ? true : false;
            }
        }
    }
    public class ItemsRequest
    {
        [Required]
        [StringLength(18, MinimumLength = 8)]
        [DefaultValue("10207953")]
        public string ItemNo { get; set; } = "10207953";

        [Required]
        [StringLength(10, MinimumLength = 1)]
        [DefaultValue("CHA")]
        public string Uom { get; set; } = "CHA";

        [Required]
        [Range(0.0001, 999.99)]
        [DefaultValue(1)]
        public decimal Quantity { get; set; } = 1;
    }
}
