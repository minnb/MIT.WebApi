using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.WebApi.GPAY.ViewModels.MakeOrder
{
    public class RspGetOrderMOR
    {
        public OrderMOR Data { get; set; }
    }
    public class OrderMOR
    {
        public string OrderCode { get; set; }
        public string Status { get; set; }
        public string StatusName { get; set; }
        public string PartnerKiosk { get; set; }
        public string PartnerKioskName { get; set; }
        public string OrderTime { get; set; }
        public string PayTime { get; set; }
        public int TotalItem { get; set; }
        public decimal TotalBill { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerName { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
        public string Message { get; set; }
        public List<RspItemMOR> Items { get; set; }
    }
    public class RspItemMOR
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public string ItemNo { get; set; }
        public string ItemNo2 { get; set; }
        public string SaleUoM { get; set; }
        public string SaleUoMName { get; set; }
        public string ItemName { get; set; }
        public decimal ItemPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public string Note { get; set; }
        public Guid ParentId { get; set; }
        public int VatPercent { get; set; }
        public string VatCode { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionType { get; set; }
        public string PromotionLine { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
    }
}
