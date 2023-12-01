using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.SalesPartner
{
    public class ItemSalesOnApp
    {
        public string PartnerCode { get; set; }
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string ItemNo { get; set; }
        public string ParentCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string Uom { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsSync { get; set; }
        public bool IsSales { get; set; }
        public bool Blocked { get; set; }
        public string Size { get; set; }
        public string ItemGroup { get; set; }
        public string ItemGroupName { get; set; }
        public bool IsApplyAll { get; set; }
        public bool IsTopping { get; set; }
        public int PictureId { get; set; }
        public int DishId { get; set; }
        public string SyncResults { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime ChgeDate { get; set; }
        public string Id { get; set; }
        public string CupType { get; set; }
        public string Action { get; set; }
    }
    public class ListItemSalesOnApp: ItemSalesOnApp
    {
        public int StoreId { get; set; }
    }
}
