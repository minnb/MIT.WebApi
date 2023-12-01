using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class Dish_get_group_detail_request
    {
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        public int dish_group_id { get; set; }
        public string partner_dish_group_id { get; set; }

    }
    public class Dish_get_group_detail
    {
        public int restaurant_id { get; set; }
        public int dish_group_id { get; set; }
        public string partner_dish_group_id { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public int display_order { get; set; }
        public string description { get; set; }
        public bool is_active { get; set; }

    }
}
