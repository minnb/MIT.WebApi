using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class Sale_Promo_Header
    {
        public int id { get; set; }
        public int? message_main_attachment_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool? active { get; set; }
        public string list_type { get; set; }
        public bool? use_for_coupon { get; set; }
        public string apply_type { get; set; }
        public string order_type { get; set; }
        public DateTime start_date_active { get; set; }
        public DateTime end_date_active { get; set; }
        public decimal start_hour { get; set; }
        public decimal end_hour { get; set; }
        public bool? first_get_flag { get; set; }
        public bool? compile_flag { get; set; }
        public int? currency_id { get; set; }
        public int? company_id { get; set; }
        public string state { get; set; }
        public string search_product_ean { get; set; }
        public int? requested_by { get; set; }
        public int? approved_by { get; set; }
        public int? create_uid { get; set; }
        public DateTime create_date { get; set; }
        public int? write_uid { get; set; }
        public DateTime write_date { get; set; }
        public int? pos_payment_method_id { get; set; }
    }
}
