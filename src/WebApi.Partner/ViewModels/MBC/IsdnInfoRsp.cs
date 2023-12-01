using System;

namespace WebApi.Partner.ViewModels.MBC
{
    public class IsdnInfoResponse
    {
        public IsdnInfoRsp isdnInfoResponse { get; set; }
    }
    public class IsdnInfoRsp
    {
        public string isdn { get; set; }
        public string status { get; set; }
        public int statusId { get; set; }
        public string type { get; set; }
        public int priceDefault { get; set; }
        public int priceCustom { get; set; }
        public DateTime createDate { get; set; }
        public string shopCode { get; set; }
        public string objectHolding { get; set; }
        public int roleId { get; set; }
        public string stock { get; set; }
        public int sold { get; set; }
        public string soldName { get; set; }
    }
}
