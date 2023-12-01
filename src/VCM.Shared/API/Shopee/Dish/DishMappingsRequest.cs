using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishMappingsRequest
    {
        [Required]
        [DefaultValue(1000007107)]
        public int restaurant_id { get; set; } = 1000007107;
        public List<DishesMapping> dishes { get; set; }
    }
    public class DishesMapping
    {
        public int dish_id { get; set; }
        public string partner_dish_id { get; set; }
    }
}
