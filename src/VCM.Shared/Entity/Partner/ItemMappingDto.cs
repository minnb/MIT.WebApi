using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Partner
{
    [Table("M_ItemMapping")]
    public class ItemMapping : ItemMappingDto
    {
    }
    public class ItemMappingDto
    {
        public string PartnerCode { get; set; }
        public string AppCode { get; set; }
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public string TaxGroupCode { get; set; }
        public int VatPercent { get; set; }
        public string Size { get; set; }
        public string ItemNo2 { get; set; }
        public string ItemGroup { get; set; }
        public string ItemName2 { get; set; }
        public int DisplayOrder { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string PartnerItemGroup { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string PictureUrl { get; set; }
    }
}
