using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Topping
{
    public class ToppingGroupMappingRequest
    {
        [Required]
        [DefaultValue(1000007107)]
        public int restaurant_id { get; set; } = 1000007107;
        public List<topping_groups_mapping> topping_groups { get; set; }
    }
    public class topping_groups_mapping
    {
        public string partner_topping_group_id { get; set; }
        public int topping_group_id { get; set; }
    }
}
