using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Topping
{
    public class ToppingMappingRequest
    {
        [Required]
        [DefaultValue(1000007107)]
        public int restaurant_id { get; set; } = 1000007107;
        public List<ToppingMapping> toppings { get; set; }
    }
    public class ToppingMapping
    {
        public int topping_id { get; set; }
        public string partner_topping_id { get; set; }
    }
}
