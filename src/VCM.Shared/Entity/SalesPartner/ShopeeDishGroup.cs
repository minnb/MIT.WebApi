using System;
using System.ComponentModel.DataAnnotations.Schema;
using VCM.Shared.API.Shopee.Dish;


namespace VCM.Shared.Entity.SalesPartner
{
    [Table("Shopee_dish_group")]
    public class ShopeeDishGroup: DishGroupShopee
    {
        public string partner_restaurant_id { get; set; }
        public DateTime crt_date { get; set; }
    }

    [Table("Shopee_dish_group_detail")]
    public class ShopeeDishGroupDetail : Dish_get_group_detail
    {
        public string partner_restaurant_id { get; set; }
        public DateTime crt_date { get; set; }
    }
}
