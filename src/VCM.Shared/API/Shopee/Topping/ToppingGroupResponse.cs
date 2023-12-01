using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Topping
{
    public class ToppingGroupResponse
    {
        public List<get_topping_groups> topping_groups { get; set; }

    }
    public class get_topping_groups
    {
        public int topping_group_id { get; set; }
        public string partner_topping_group_id { get; set; }
        public string name { get; set; }
        public int display_order { get; set; }
        public bool is_active { get; set; }       
        public string name_en { get; set; }
        public string description { get; set; }
    }
}