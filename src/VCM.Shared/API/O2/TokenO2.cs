using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.O2
{
    public class UserLoginO2
    {
        public string ConsumerKey { get; set; }
        public string Password { get; set; }
    }
    public class TokenO2
    {
        public DataTokenWincustomer Data { get; set; }
        //public string Message { get; set; }
        //public DateTime Time { get; set; }
        //public string Verdict { get; set; }
    }
    public class DataTokenO2
    {
        public string access_token { get; set; }
    }

    public class DataTokenWincustomer
    {
        public string Token { get; set; }
    }
}
