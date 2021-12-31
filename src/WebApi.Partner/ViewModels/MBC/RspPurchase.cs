using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.ViewModels.MBC
{
    public class RspPurchaseCard
    {
        public int total { get; set; }
        public List<PurchaseItem> voucherList { get; set; }

    }
    public class PurchaseItem
    {
        public string code { get; set; }
        public string value { get; set; }
        public string voucherId { get; set; }
        public VoucherStatus voucherStatus {get;set;}
        public string serial { get; set; }
        public string expireDate { get; set; }
    }
    public class VoucherStatus
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
    public class ResponseTopup
    {
        public string transId { get; set; }
    }
}
