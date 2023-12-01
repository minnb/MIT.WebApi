using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.BLUEPOS
{
    public class RspWebApiBluePOS
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

}
