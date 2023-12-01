using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Topping
{
    public class ToppingGetResponse
    {
       public List<toppings> toppings { get; set; }

    }
    public class toppings
    {
        public int topping_id { get; set; }
        public int topping_group_id { get; set; }
        public string partner_topping_id { get; set; }
        public string partner_topping_group_id { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public int display_order { get; set; }
        public decimal price { get; set; }
        public bool is_active { get; set; }
        
    }
}
