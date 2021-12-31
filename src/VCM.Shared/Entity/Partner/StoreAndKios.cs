using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Entity.Partner
{
    [Table("M_StoreAndKios")]
    public class StoreAndKios
    {
        public int WarehouseId { get; set; }
        public int LocationId { get; set; }
        public string PosOdoo { get; set; }
        public string LocationName { get; set; }
        public string StoreNo { get; set; }
        public string StoreName { get; set; }
        public string Ver { get; set; }
        public string QRCode { get; set; }
        public bool Blocked { get; set; }
        public int Subset { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
