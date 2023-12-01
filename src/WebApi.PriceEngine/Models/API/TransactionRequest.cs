using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VCM.Shared.API.Attribute;

namespace WebApi.PriceEngine.Models.API
{
    public class TransactionRequest
    {
        [Required]
        [StringLength(15, MinimumLength = 3)]
        [StringRange(AllowableValues = new[] { "WCM", "PLH", "PLF" }, ErrorMessage = "AppCode không đúng (WCM, PLH, PLF)")]
        [DefaultValue("WCM")]
        public string AppCode { get; set; } = "WCM";
        [Required]
        [StringLength(6, MinimumLength = 4)]
        [DefaultValue("2018")]
        public string StoreNo { get; set; } = "2018";
        [Required]
        [StringLength(20)]
        public string OrderNo { get; set; }
        [Required]
        [StringLength(8)]
        public string OrderDate { get; set; }
        [Required]
        [StringLength(50)]
        public string RequestId { get; set; }
        public bool IsLoyalty { get; set; }
        public string WinCode { get; set; }
        public IEnumerable<ItemRequest> Items { get; set; }
    }

    public class ItemRequest
    {
        [Required]
        public int LineNo { get; set; }
        [Required]
        public int ParentLineNo { get; set; }
        public string Barcode { get; set; }
        [Required]
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        [Required]
        public string Uom { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        [Required]
        public string TaxGroupCode { get; set; }
        [Required]
        public int VatPercent { get; set; }

        [StringRange(AllowableValues = new[] { "", "PAPER", "PLASTIC" }, ErrorMessage = "Loại LY sử dụng 'PAPER' or 'PLASTIC'")]
        public string CupType { get; set; } = String.Empty;
        [StringRange(AllowableValues = new[] { "", "H", "M", "L" }, ErrorMessage = "Size sản phẩm không đúng H, M, L")]
        public string Size { get; set; } = String.Empty;
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public string ComboNo { get; set; }
    }
}
