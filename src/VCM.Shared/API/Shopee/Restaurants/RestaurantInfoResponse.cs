using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Restaurants
{
    public class RestaurantInfoResponse
    {
        public List<RestaurantHeader> restaurants { get; set; }   
    }

    public class RestaurantHeader
    {
        public string city { get; set; }
        public string foody_service { get; set; }
        public string name { get; set; }
        public int restaurant_id { get; set; }
        public string address { get; set; }
        public string partner_restaurant_id { get; set; }
    }

    public class Store_operation_time_ranges
    {
       public List<operation_time_ranges> days { get; set; }
    }

    public class operation_time_ranges
    {
        public string day_of_week { get; set; }
        public string custom_date { get; set; }
        public bool is_closed { get; set; }
        public List<time_ranges> time_ranges { get; set; }
    }
    public class time_ranges
    {
        public int id { get; set; }
        public string open_time { get; set; }
        public bool close_time { get; set; }
    }


}
