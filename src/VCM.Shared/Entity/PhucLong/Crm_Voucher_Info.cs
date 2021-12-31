using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class Crm_Voucher_Info
    {
        public int id { get; set; }
        public string ean { get; set; }
        public DateTime publish_date { get; set; }
        public int? company_id { get; set; }
        public int? customer_id { get; set; }
        public int publish_id { get; set; }
        public string state { get; set; }
        public DateTime? effective_date_from { get; set; }
        public DateTime? effective_date_to { get; set; }
        public int? promotion_line_id { get; set; }
        public decimal? voucher_amount { get; set; }
        public string type { get; set; }
        public string order_reference { get; set; }
        public int? usage_limits { get; set; }
        public int? used_count { get; set; }
        public int? create_uid { get; set; }
        public DateTime create_date { get; set; }
        public int? write_uid { get; set; }
        public DateTime write_date { get; set; }
        public int? warehouse_id { get; set; }
        public bool? apply_for_employee { get; set; }
        public int? employee_id { get; set; }
        public int? appear_code_id { get; set; }
        public int? pos_order_id { get; set; }
        public DateTime? date_used { get; set; }
        public string product_coupon_order_ref { get; set; }
        public decimal? reward_point_cost { get; set; }
        public int? from_pos_id { get; set; }
        public int? partner_id { get; set; }
        public int? loyalty_level_id { get; set; }
        public int? loyalty_reward_id { get; set; }
        public string used_on { get; set; }
    }
}
