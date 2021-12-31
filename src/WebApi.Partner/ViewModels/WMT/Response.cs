using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.ViewModels.WMT
{
    public class RespHeaderWMT
    {
        public int ResponseCode { get; set; }
        public object TechnicalMessage { get; set; }
        public string Message { get; set; }
    }
    public class RspListOrderWMT
    {
        public List<OrderHeaderWMT> Data { get; set; }
    }
    public class OrderHeaderWMT
    {
        public string OrderCode { get; set; }
        public string Status { get; set; }
        public string OrderTime { get; set; }
        public string VINIDNumber { get; set; }
        public decimal TotalPayment { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverAddress { get; set; }
    }

    public class RspOrderDetailWMT
    {
       public DetailOrderWMT Data { get; set; }
    }
    public class DetailOrderWMT
    {
        public string OrderCode { get; set; }
        public string Status { get; set; }
        public string OrderTime { get; set; }
        public string VINIDNumber { get; set; }
        public decimal BillAmount { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverAddress { get; set; }
        public List<ItemWMT> Items { get; set; }
    }
    public class ItemWMT
    {
        public string ItemNo { get; set; }
        public string BarcodeNo { get; set; }
        public string UOM { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal VatPercent { get; set; }
        public string VatGroup { get; set; }
        public decimal VatAmount { get; set; }
        public string ItemType { get; set; }
        public string ItemID { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionName { get; set; }
    }
    public class RspUpdateStatusOrderlWMT 
    {
        public string Message { get; set; }
    }
    public class RspUpdateOrderWMT
    {
        public string OrderCode { get; set; }
    }
    public class RspBadRequest
    {
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
    }
}