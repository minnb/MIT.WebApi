using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishGetOutOfServiceRequest
    {
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        [Required]
        public string[] partner_dish_ids { get; set; }
    }
    public class DishGetOutOfServiceResponse
    {
        public List<DishGetOutOfServiceResult> dishes { get; set; }
    }
    public class DishGetOutOfServiceResult
    {
        public int dish_id { get; set; }
        public string partner_dish_id { get; set; }
        public string from_time { get; set; }
        public string to_time { get; set; }
    }

}
