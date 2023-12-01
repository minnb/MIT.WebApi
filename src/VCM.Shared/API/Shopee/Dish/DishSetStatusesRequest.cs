using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Dish
{
    public class DishSetStatusesRequest
    {
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        public bool is_apply_all { get; set; }
        public List<ArrDishSetStatuses> dishes { get; set; }

    }
    public class DishSetStatusRequestAll: DishSetStatusesRequest
    {
        public int[] branch_ids { get; set; }
    }
    public class ArrDishSetStatuses
    {
        [Required]
        public string partner_dish_id { get; set; }
        [Required]
        public int status { get; set; } 
        //YYYY-MM-DD hh:mm:ss
        //public string from_time { get; set; } //YYYY-MM-DD hh:mm:ss
        //public string to_time { get; set; }
    }

    //AVAILABLE=1
    //OUT_OF_STOCK=2 I
    //NACTIVE = 3


}
