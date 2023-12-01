using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VCM.Shared.API.Shopee.Restaurants;

namespace VCM.Shared.Entity.SalesPartner
{
    [Table("Shopee_restaurant")]
    public class ShopeeRestaurant : RestaurantHeader
    {
        public bool is_header { get; set; }
        public string parent_restaurant   { get; set; }
    }

}
