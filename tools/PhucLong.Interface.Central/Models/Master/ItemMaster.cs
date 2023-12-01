using PhucLong.Interface.Central.Models.Master.SAP;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PhucLong.Interface.Central.Models.Master
{
    public class ItemMaster: DataItemXml
    {
        public string FileIdoc { get; set; }
    }

    [Table("Temp_Item")]
    public class Temp_Item : ItemMaster
    {
        public string UpdateFlg { get; set; }
        public string Id { get; set; }
    }

    [Table("Master_Item")]
    public class MasterItem
    {
        public string ItemNo { get; set; }
        public string ItemNo2 { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public string TaxGroupCode { get; set; }
        public decimal VatPercent { get; set; }
        public string MCH { get; set; }
        public string SIZE_DIM { get; set; }
        public string BASIC_MATL { get; set; }
        public bool Blocked { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
