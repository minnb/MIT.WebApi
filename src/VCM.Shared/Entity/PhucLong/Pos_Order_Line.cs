using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Pos_Order_Line
    {
        public int id { get; set; }
        public string name { get; set; }
        public int product_id { get; set; }
        public decimal price_unit { get; set; }
        public decimal qty { get; set; }
        public decimal price_subtotal { get; set; }
        public decimal price_subtotal_incl { get; set; }
        public decimal discount { get; set; }
        public int order_id { get; set; }
        public DateTime create_date { get; set; }
        public bool is_promotion_line { get; set; }
        public bool is_condition_line { get; set; }
        public int promotion_id { get; set; }
        public int promotion_condition_id { get; set; }
        public int uom_id { get; set; }
        public decimal old_price { get; set; }
        public bool is_manual_discount { get; set; }
        public decimal return_discount { get; set; }
        public decimal loyalty_discount_percent { get; set; }
        public decimal promotion_line_id { get; set; }
        public int fe_uid { get; set; }
        public bool is_loyalty_line { get; set; }
        public bool is_birthday_promotion { get; set; }
        public string note { get; set; }
        public decimal discount_amount { get; set; }
        public bool is_topping_line { get; set; }
        public int related_line_id { get; set; }
        public bool disable_promotion { get; set; }
        public bool disable_loyalty { get; set; }
        public int partner_id { get; set; }
        public decimal old_price_total { get; set; }
        public int warehouse_id { get; set; }
        public DateTime date_order { get; set; }
        public decimal loyalty_point_cost { get; set; }
        public int combo_id { get; set; }
        public bool is_done_combo { get; set; }
        public string combo_seq { get; set; }
        public string cup_type { get; set; }
        public decimal amount_surcharge { get; set; }
        public string combo_qty { get; set; }
        public int reward_id { get; set; }
        public string cashless_code { get; set; }
        public string product_coupon_code { get; set; }
        public string code { get; set; }
        public decimal plh_redeem { get; set; }
        public decimal plh_earn { get; set; }
        public decimal cv_life_redeem { get; set; }
        public decimal cv_life_earn { get; set; }
    }
}
