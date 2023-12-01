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
    public class wsRequestCreateEsimOrder: wsRequestUser
    {
        public string isdn { set; get; }
    }
    public class wsRequestSerial: wsRequestUser
    {
        public string serial { get; set; }
    }

    public class wsCreateEsimOrder : wsRequestUser
    {
        public string isdn { get; set; }
        public int packageId { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }
    public class wsCreatePhysicalSimOrder : wsRequestUser
    {
        public string isdn { get; set; }
        public int mainPackId { get; set; }
        public string serial { get; set; }
    }
    public class rpCreatePhysicalSimOrder 
    {
        public string orderCode { get; set; }
        public string isdn { get; set; }
        public string guideLineUrl { get; set; }
        public string imSerial { get; set; }
        public int mainPackagePrice { get; set; }
        public int priceSim { get; set; }
        public string kycExpireDate { get; set; }
    }
    public class wsKeepIsdn: wsRequestUser
    {
        public string isdn { get; set; }
    }

    public class validateSimStatus : wsRequestUser
    {
        public string serial { get; set; }
        public string type { get; set; }
    }


    public class wsRequestUser
    {
        public string username { get; set; }
        public string accountId { get; set; }
        public string storeId { get; set; }
    }
    public class wsRequestCheckExtendSubscriberInfo
    {
        public string isdn { get; set; }
        public bool displayedAll { get; set; }
        public string username { get; set; }
        public string accountId { get; set; }
    }
    public class wsRequestValidateKitStatus
    {
        public string storeId { get; set; }
        public string serial { get; set; }
    }
    public class wsUpdateKitStatusKYC
    {
        public string storeId { get; set; }
        public string serial { get; set; }
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

    public class validateKitStatusRsp
    {
        public string kitSerial { get; set; }
        public string packageName { get; set; }
    }
    public class subscriberInfoRsp
    {
        public string isdn { get; set; }
        public string customerName { get; set; }
        public int ocsMainBalance { get; set; }
        public List<packagesKitWintel> packages { get; set; }

    }
    public class packagesKitWintel
    {
        public string packageName { get; set; }
        public string registeredDate { get; set; }
        public string expiredDate { get; set; }
        public string status { get; set; }
        public int price { get; set; }
        public int missingAmount { get; set; }
        public string message { get; set; }
    }
}
