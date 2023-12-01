using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee
{
    public class OrderListShopeeRsp
    {
        public string result { get; set; }
        public ReplyOderList reply { get; set; }
    }
    public class ReplyOderList
    {
        public List<OrderListShopee> orders { get; set; }
    }
    public class OrderListShopee
    {
        public int status { get;set;}
        public string distance { get; set; }
        public string code { get; set; }
        public order_value order_value { get; set; }
        public DateTime order_time { get; set; }
        public Restaurant restaurant { get; set; }
        public total_value total_value { get; set; }
        public DateTime delivery_time { get; set; }
        public DateTime pick_time { get; set; }
        public pay_to_merchant pay_to_merchant { get; set; }
    }
    public class pay_to_merchant
    {
        public int status { get; set; }
        public int type { get; set; }
    }
    public class temp_value
    {
        public string text { get; set; }
        public decimal value { get; set; }
        public string unit { get; set; }
    }
    public class total_value: temp_value
    {
    }
    public class Restaurant
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class order_value : temp_value
    {
    }
}
