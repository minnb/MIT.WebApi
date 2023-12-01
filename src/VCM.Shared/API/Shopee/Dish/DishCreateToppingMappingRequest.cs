using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishCreateToppingMappingRequest
    {
        [Required]
        public int restaurant_id { get; set; }
        [Required]
        public string partner_restaurant_id { get; set; }
        [Required]
        public string partner_dish_id { get; set; }
        public bool is_apply_all { get; set; }
        public List<DishCreateToppingMapping> toppings { get; set; }
    }
    public class DishCreateToppingMapping
    {
        [Required]
        public string partner_topping_id { get; set; }
        [Required]
        public decimal price { get; set; }
        public bool is_required { get; set; }
    }
}
