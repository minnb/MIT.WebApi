using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VCM.Shared.API.Shopee.Topping;

namespace VCM.Shared.API.Shopee.Dish
{
    public class Get_approval_status : restaurant_id_now
    {
        public string partner_dish_id { get; set; }
    }
    public class Get_approval_status_rsp
    {
        public int status { get; set; }
        public int dish_id { get; set; }
    }
}
