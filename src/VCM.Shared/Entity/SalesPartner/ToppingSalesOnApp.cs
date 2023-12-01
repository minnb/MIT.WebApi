using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.SalesPartner
{
    public class ToppingSalesOnApp
    {
        public string PartnerCode { get; set; }
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string ToppingGroup { get; set; }
        public string ToppingNo { get; set; }
        public string Name { get; set; }
        public string Name_En { get; set; }
        public decimal UnitPrice { get; set; }
        public bool DisplayOrder { get; set; }
        public bool IsApplyAll { get; set; }
        public bool Blocked { get; set; }
        public bool IsSync { get; set; }
        public string SyncResults { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime ChgeDate { get; set; }
        public string Id { get; set; }
        public string Action { get; set; }
    }
    public class ListToppingSalesOnApp : ToppingSalesOnApp
    {
        public int StoreId { get; set; }
    }
}
