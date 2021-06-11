using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.WebApi.GPAY.ViewModels.AirPay
{
    public class RespCode
    {
        public int resp_code { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string resp_msg { get; set; }
    }
}
