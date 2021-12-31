using System.Collections.Generic;

namespace VCM.Partner.API.ViewModels.MBC
{
    public class RequestMBC
    {
        public string sessionId { get; set; }
        public string username { get; set; }
        public string apiKey { get; set; }
        public string wsCode { get; set; }
        public object wsRequest { get; set; }
    }
    public class WsRequestGetListBill
    {
        public string storeId { get; set; }
        public string username { get; set; }
        public string accountId { get; set; }
    }
    public class WsRequestGetBillDetail
    {
        public string storeId { get; set; }
        public string billCode { get; set; }
    }
    public class WsRequestChangePaymentStatus
    {
        public string storeId { get; set; }
        public string billCode { get; set; }
        public int paymentStatus { get; set; }
    }

    public class ResponseMBC
    {
        public int errorCode { get; set; }
        public string message { get; set; }
        public object wsResponse { get; set; }
    }
    public class BillDetail
    {
        public BillHeader header { get; set; }
        public List<BillLines> lines { get; set; }
        public int paymentStatus { get; set; }

    }
    public class RspListBillMBC
    {
        public int errorCode { get; set; }
        public string message { get; set; }
        public ListBill wsResponse { get; set; }
    }
    public class ListBill
    {
        public List<BillHeader> listBill { get; set; }
    }
    public class BillHeader
    {
        public string billCode { get; set; }
        public string createdAt { get; set; }
        public int storeId { get; set; }
        public string customerName { get; set; }
        public string customerPhone { get; set; }
        public decimal totalPrice { get; set; }
        public bool requireSerial { get; set; }
    }
    public class BillLines
    {
        public string productCode { get; set; }
        public string productNumber { get; set; }
        public string productName { get; set; }
        public string unit { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public int tax { get; set; }
        public decimal totalPrice { get; set; }
        public string phoneNumber { get; set; }
        public bool requireSerial { get; set; }
        public List<VoucherMBC> vouchers { get; set; }
    }

    public class VoucherMBC
    {
        public string vouchernumber { get; set; }
        public string comp { get; set; }
    }

}
