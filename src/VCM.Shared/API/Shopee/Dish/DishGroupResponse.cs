using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishGroupResponse
    {
        public List<DishGroupShopee> dish_groups { get; set; }
      
    }
    public class DishGroupShopee
    {
        public int dish_group_id { get; set; }
        public string partner_dish_group_id { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public string description { get; set; }
        public int display_order { get; set; }
    }
}
