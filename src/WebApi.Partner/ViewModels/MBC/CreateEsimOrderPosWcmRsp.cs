using System;
using VCM.Shared.API.Wintel;

namespace WebApi.Partner.ViewModels.MBC
{
    public class CreateEsimOrderPosWcmRequest: ValidateKitStatusRequest
    {
        public int PackageId { get; set; }
        public string OrderNo { get; set; }
        public string CustEmail { get; set; }
        public string CustPhone { get; set; }
    }
    public class CreatePhysicalSimOrderPosWcmRequest : ValidateKitStatusRequest
    {
        public string SerialSimKIT { get; set; }
        public int PackageId { get; set; }
        public string OrderNo { get; set; }
        public string CustEmail { get; set; }
        public string CustPhone { get; set; }
    }

    public class CreateEsimOrderPosWcmRsp
    {
        public string OrderCode { get; set; }
        public string Isdn { get; set; }
        public string IpaString { get; set; }
        public string GuideLineUrl { get; set; }
        public string ImSerial { get; set; }
        public decimal MainPackagePrice { get; set; }
        public decimal PriceSim { get; set; }
        public string KycExpireDate { get; set; }
    }
    public class CreatePhysicalSimOrderPosWcmRsp
    {
        public string OrderCode { get; set; }
        public string Isdn { get; set; }
        public string MainPackId { get; set; }
        public string Serial { get; set; }
    }
}
