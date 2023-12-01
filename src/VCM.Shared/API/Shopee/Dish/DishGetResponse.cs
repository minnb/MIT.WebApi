using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishGetResponse
    {
        public List<DishesShopee> dishes { get; set; }
    }
    public class DishesShopee
    {
        public int dish_id { get; set; }
        public int dish_group_id { get; set; }
        public string partner_dish_id { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public int display_order { get; set; }
        public decimal price { get; set; }
        public string description { get; set; }
        public bool is_active { get; set; }
        public string partner_dish_group_id { get; set; }
        public DateTime? created_time { get; set; }
        public DateTime? updated_time { get; set; }
        public DishPicture picture { get; set; }
    }
}
