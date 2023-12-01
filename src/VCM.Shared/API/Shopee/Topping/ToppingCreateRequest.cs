using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Topping
{
    public class ToppingCreateRequest
    {
        [Required]
        public int restaurant_id { get; set; }
        [Required]
        public string partner_restaurant_id { get; set; }
        [Required]
        public string partner_topping_id { get; set; }
        [Required]
        public string partner_topping_group_id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public decimal price { get; set; }
        public string name_en { get; set; }
        public int display_order { get; set; }
        public bool is_apply_all { get; set; }
    }
    public class ToppingCreateResponse
    {
        public int topping_id { get; set; }
        public bool is_pending { get; set; }
    }
}
