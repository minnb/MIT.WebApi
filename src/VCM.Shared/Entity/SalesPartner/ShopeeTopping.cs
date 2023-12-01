using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VCM.Shared.API.Shopee.Topping;

namespace VCM.Shared.Entity.SalesPartner
{
    [Table("Shopee_topping")]
    public class ShopeeTopping: toppings
    {
        public string partner_restaurant_id { get; set; }
    }


    [Table("Shopee_topping_group")]
    public class ShopeeToppingGroup : get_topping_groups
    {
        public string partner_restaurant_id { get; set; }
    }
}
