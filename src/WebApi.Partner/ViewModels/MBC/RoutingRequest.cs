using System;
using System.Collections.Generic;

namespace VCM.Partner.API.ViewModels.MBC
{
    public class RoutingRequest
    {
        public string function { get; set; }
        public string checksum { get; set; }
        public string version { get; set; }
        public string merchantId { get; set; }
        public string encData { get; set; }
    }
    public class TripleObj
    {
        //public wsRequestLstBill wsRequest { get; set; }
        public Object wsRequest { get; set; }
    }
    public class wsRequestLstBill
    {
        public string storeId { get; set; }
        public string username { get; set; }
        public string accountId { get; set; }
    }
    public class wsRequestBillDetail
    {
        public string storeId { get; set; }
        public string billCode { get; set; }
        public string username { get; set; }
        public string accountId { get; set; }
    }
    public class wsRequestPurchaseCard
    {
        public string username { get; set; }
        public string accountId { get; set; }
        public int value { get; set; }
        public int quantity { get; set; }
        public string transId { get; set; }
        public string shopId { get; set; }
        public string storeId { get; set; }
    }
    public class wsRequestTopupCard
    {
        public string username { get; set; }
        public string accountId { get; set; }
        public int value { get; set; }
        public string msisdn { get; set; }
        public string shopId { get; set; }
        public string storeId { get; set; }
        public string transId { get; set; }
    }
    public class wsRequestSerial
    {
        public string storeId { get; set; }
        public string serial { get; set; }
        public string username { get; set; }
        public string accountId { get; set; }
    }
    public class wsRequestUpdateStatus
    {
        public string storeId { get; set; }
        public string billCode { get; set; }
        public int paymentStatus { get; set; }
        public IList<listSim> listSim { get; set; }
        public string username { get; set; }
        public string accountId { get; set; }
    }

    public class wsRequestCancelOrder
    {
        public string storeId { get; set; }
        public string billCode { get; set; }
        public string username { get; set; }
        public string accountId { get; set; }
    }
    public class listSim
    {
        public string serial { get; set; }
        public string code { get; set; }
    }
    public class ResponseEncData
    {
        public string code { get; set; }
        public string message { get; set; }
        public string checksum { get; set; }
        public string encData { get; set; }
    }
}
