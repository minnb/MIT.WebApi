using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.SalesPartner
{
    public class ActionLogging
    {
        public string PartnerCode { get; set; }
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public decimal UnitPrice { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public DateTime CrtDate  { get; set; }
        public DateTime ChgeDate { get; set; }
        public string RefId { get; set; }
        public long Id { get; set; }
    }
}
