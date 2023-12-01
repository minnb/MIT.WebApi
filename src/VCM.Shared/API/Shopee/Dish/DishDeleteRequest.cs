using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishDeleteRequest
    {
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        public string partner_dish_id { get; set; }
    }

    public class DishDeleteResponse
    {
       public bool is_pending { get; set; }
    }
}
