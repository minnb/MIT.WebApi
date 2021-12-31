using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Entity.Partner
{
    [Table("M_Item")]
    public class Item
    {
        public string AppCode { get; set; }
        public string PartnerItem { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public string Uom { get; set; }
        public string VatGroup { get; set; }
        public int VatPercent { get; set; }
        public string ItemType { get; set; }
        public string RefNo { get; set; }
        public bool Blocked { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
