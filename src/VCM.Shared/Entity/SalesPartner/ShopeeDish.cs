using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.SalesPartner
{
    [Table("Shopee_dish")]
    public class ShopeeDish
    {
        public string partner_restaurant_id { get; set; }
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
        public int picture_id { get; set; }
        public string url { get; set; }
    }


}
