using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.SalesPartner
{
    public class ItemToppingMapping
    {
        public string PartnerCode { get; set; }
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string ItemNo { get; set; }
        public string ToppingNo { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsRequired { get; set; }
        public bool IsApplyAll { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public bool Blocked { get; set; }
        public bool IsSync { get; set; }
        public bool IsSetQuantity { get; set; }
        public string SyncResults { get; set; }
        public string SyncSetQuantity { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime ChgeDate { get; set; }
        public string Id { get; set; }
        public string Action { get; set; }
    }
    public class ListItemToppingMapping : ItemToppingMapping
    {
        public int StoreId { get; set; }
    }
}
