using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Topping
{
    public class topping_update_prices
    {
        [Required]
        public int restaurant_id { get; set; }
        [Required]
        public string partner_restaurant_id { get; set; }
        [Required]
        public string partner_topping_group_id { get; set; }
        public bool is_apply_all { get; set; } = true;
        public List<List_topping_update_prices> toppings { get; set; }
    }

    public class List_topping_update_prices
    {
        public string partner_topping_id { get; set; }
        public string partner_dish_id { get; set; }
        public decimal price { get; set; }

    }
    public class topping_set_group_quantity: restaurant_id_now
    {
        [Required]
        public string partner_topping_group_id { get; set; }
        [Required]
        public List<dishes_topping_set_group_quantity> dishes { get; set; }       
        public bool is_apply_all { get; set; }
        //public int[] branch_ids { get; set; }
    }

    public class dishes_topping_set_group_quantity
    {
        [Required]
        public string partner_dish_id { get; set; }
        [Required]
        public int min_quantity { get; set; }
         [Required]
        public int max_quantity { get; set; }

    }

    public class restaurant_id_now
    {
        [Required]
        [DefaultValue(1000007107)]
        public int restaurant_id { get; set; } = 1000007107;
        [Required]
        [DefaultValue("2001")]
        public string partner_restaurant_id { get; set; } = "2001";
    }
}
