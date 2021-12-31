using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    public class TransCancel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OrderNo { get; set; }
        public string RefNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string State { get; set; }
        public int LocationId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime DateLastOrder { get; set; }
        public DateTime WriteDate { get; set; }
        public string Note { get; set; }
        public string NoteLabel { get; set; }
        public string UpdateFlg { get; set; }
        public bool IsEOD { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
