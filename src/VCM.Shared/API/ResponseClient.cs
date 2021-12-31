using Newtonsoft.Json;
using System;

namespace VCM.Shared.API
{
    public class ResponseClient
    {
        [JsonProperty("Meta")]
        public Meta Meta { get; set; }
        [JsonProperty("Meta")]
        public Object Data { get; set; }
    }
    public class Meta
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
