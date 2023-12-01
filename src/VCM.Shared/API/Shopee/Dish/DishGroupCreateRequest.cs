using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishGroupCreateRequest
    {
        [Required]
        public int restaurant_id { get; set; }
        [Required]
        public string partner_restaurant_id { get; set; }
        [Required]
        public string partner_dish_group_id { get; set; }
        [Required]
        public string name { get; set; }
        public string name_en { get; set; }
        public int display_order { get; set; }
        public string description { get; set; }
        public bool is_apply_all { get; set; }
    }
    public class DishGroupCreateResponse
    {
        public int dish_group_id { get; set; }
    }
    public class dish_partner_group_id_mappings_response
    {
        public List<DishGroupsInfo> dish_groups { get; set; }
    }
    public class DishGroupsInfo
    {
        public int dish_group_id { get; set; }
        public int? pos_id { get; set; }
        public string partner_dish_group_id { get; set; }
        public string name { get; set; }
    }
}
