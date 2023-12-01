using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee
{
    public class OrderUpdateShopee
    {
        public string order_code { get; set; }
        public string serial { get; set; }
        public int status { get; set; }
        public int restaurant_id { get; set; }
    }
    public class OrderDetailShopeeRequest
    {
        public string order_code { get; set; }
    }
    public class OrderUpdateShopeeRequest
    {
        public string order_code { get; set; }
        public string serial { get; set; }
        public int status { get; set; }

    }
    public class OrderListShopeeRequest
    {
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        public string from_date { get; set; }
        public string to_date { get; set; }
        public int limit { get; set; } = 100;
        public int offset { get; set; }
        public int status { get; set; }
    }

    public class ExamObject
    {
        public int now_order_id { get; set; }
        public int order_id { get; set; } = 100;
        public int status { get; set; }
    }
}
