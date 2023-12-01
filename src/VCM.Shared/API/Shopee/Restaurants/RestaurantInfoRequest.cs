using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Restaurants
{
    public class RestaurantInfoRequest
    {
        public string partner_restaurant_id { get; set; }
    }
    public class RestaurantInfoByIdRequest
    {
        public int restaurant_id { get; set; }
    }
}
