using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Topping
{
    public class topping_set_statuses
    {
        [Required]
        public int restaurant_id { get; set; }
        [Required]
        public string partner_restaurant_id { get; set; }
        public bool is_apply_all { get; set; }
        public List<toppings_set_status> toppings { get; set; }
    }
    public class toppings_set_status
    {
        [Required]
        public string partner_topping_id { get; set; }
        public int status { get; set; }
    }
    //AVAILABLE=1
    //OUT_OF_STOCK=2 I
    //NACTIVE = 3
}
