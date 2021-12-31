using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.ViewModels.AirPay
{
    public class RespCode
    {
        public int resp_code { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string resp_msg { get; set; }
    }
    public class DataPurchaseCard
    {
        public string signature { get; set; }
        public string trans_time { get; set; }
        public string balance { get; set; }
        public string reference_no { get; set; }
        public string apc_order_id { get; set; }
        public List<Cards> Cards { get; set; }
    }
    public class Cards
    {
        public string Serial { get; set; }
        public string Pin { get; set; }
        public string Expiry { get; set; }
    }

    public class GetCardV2Rsp
    {
        public object Order_info { get; set; }
        public string signature { get; set; }
    }
    
    public class GetBalanceRsp
    {
        public string balance { get; set; }
    }

}
