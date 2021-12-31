using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MIT.Utils.Utils
{
    public static class CommonUtils
    {
        public static string CreateRequest(string checkSum, string func, string merchantId, string version, object mbcRequest, string key)
        {
            var rep = new CreateRequestMBC()
            {
                checkSum = checkSum,
                func = func,
                merchantId = merchantId,
                version = version,
                mbcRequest = mbcRequest,
                key = key,

            };
            return JsonConvert.SerializeObject(rep);
        }
    }


    public class CreateRequestMBC
    {
        public string checkSum { get; set; }
        public string func { get; set; }
        public string merchantId { get; set; }
        public string version { get; set; }
        public object mbcRequest { get; set; }
        public string key { get; set; }
    }
}
