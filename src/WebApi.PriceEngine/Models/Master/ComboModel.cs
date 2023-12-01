using System;
using System.Collections.Generic;

namespace WebApi.PriceEngine.Models.Master
{
    public class ComboModel
    {
        public string StoreNo { get; set; }
        public string ComboNo { get; set; }
        public string ComboName { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public bool IsMember { get; set; }
        public int BuyQty { get; set; }
        public int GetQty { get; set; }
        public List<ComboItemBuy> ItemBuy { get; set; }
        public List<ComboItemGet> ItemGet { get; set; }
    }
    public class ComboHeader
    {
        public string StoreNo { get; set; }
        public string ComboNo { get; set; }
        public string ComboName { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public bool IsMember { get; set; }
        public int BuyQty { get; set; }
        public int GetQty { get; set; }
    }
    public class ComboItems
    {
        public string ComboNo { get; set; }
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string LineGroup { get; set; }
        public string ItemType { get; set; }
    }
    public class ComboItemBuy
    {
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string LineGroup { get; set; }
    }
    public class ComboItemGet
    {
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string LineGroup { get; set; }
    }
}
