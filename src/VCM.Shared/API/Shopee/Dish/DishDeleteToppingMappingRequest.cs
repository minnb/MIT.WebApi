using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishDeleteToppingMappingRequest
    {
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        public string partner_dish_id { get; set; }
        [Required]
        public string[] partner_topping_ids { get; set; }
    }
}
