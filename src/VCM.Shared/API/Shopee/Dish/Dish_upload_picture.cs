using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.API.Shopee.Topping;

namespace VCM.Shared.API.Shopee.Dish
{
    public class dish_upload_picture: restaurant_id_now
    {
        public string image_base_64 { get; set; }
        public bool is_apply_all { get; set; }
    }

    public class dish_delete_picture : restaurant_id_now
    {
        public string partner_dish_id { get; set; }
        public int picture_id { get; set; }
    }
}
