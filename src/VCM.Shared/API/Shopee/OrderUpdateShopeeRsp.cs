using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee
{
    public class OrderUpdateShopeeRsp
    {
        public string result { get; set; }
        public replyOrderUpdate reply { get; set; }
    }
    public class replyOrderUpdate
    {
        public pay_to_merchant pay_to_merchant { get; set; }
    }
}
