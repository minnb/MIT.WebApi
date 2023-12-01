using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Dtos
{
    public class api_response_meta
    {
        public response_meta meta { get; set; }
        public object data { get; set; }
    }

    public class response_meta
    {
        public int code { get; set; }
        public string message { get; set; }
    }
}
