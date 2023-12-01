using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class Dish_update_partner_group_id_mappings
    {
        [Required]
        public int restaurant_id { get; set; }
        [Required]
        public List<DishGroupUpdatePartnerMapping> dish_groups { get; set; }
       
    }
    public class DishGroupUpdatePartnerMapping
    {
        [Required]
        public int dish_group_id { get; set; }
        [Required]
        public string partner_dish_group_id { get; set; }
    }
}
