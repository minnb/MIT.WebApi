using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Pos_Order
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime date_order { get; set; }
        public int user_id { get; set; }
        public decimal amount_tax { get; set; }
        public decimal amount_total { get; set; }
        public decimal amount_paid { get; set; }
        public decimal amount_return { get; set; }
        public int pricelist_id { get; set; }
        public int partner_id { get; set; }
        public string state { get; set; }
        public int account_move { get; set; }
        public int picking_id { get; set; }
        public int location_id { get; set; }
        public string note { get; set; }
        public string pos_reference { get; set; }
        public int sale_journal { get; set; }
        public bool to_invoice { get; set; }
        public int create_uid { get; set; }
        public DateTime create_date { get; set; }
        public int employee_id { get; set; }
        public string cashier { get; set; }
        public decimal discount_amount { get; set; }
        public int invoice_id { get; set; }
        public int warehouse_id { get; set; }
        public int cashier_id { get; set; }
        public string coupon_code { get; set; }
        public int promotion_id { get; set; }
        public int picking_return_id { get; set; }
        public string return_origin { get; set; }
        public decimal loyalty_points { get; set; }
        public decimal point_won { get; set; }
        public int year_discount_birth { get; set; }
        public int sale_type_id { get; set; }
        public string note_label { get; set; }
        public bool disable_loyalty_discount { get; set; }
        public bool has_printed_label_first { get; set; }
        public string linked_draft_order_be { get; set; }
        public bool use_emp_coupon { get; set; }
        public string emp_coupon_code { get; set; }
        public int current_coupon_limit { get; set; }
        public int current_coupon_promotion { get; set; }
        public decimal total_surcharge { get; set; }
        public int number_of_printed_bill { get; set; }
        public double hanging_time { get; set; }
        public string reward_code { get; set; }
        public string momo_payment_ref { get; set; }
        public double partner_current_point { get; set; }
        public double partner_total_point { get; set; }
        public int partner_loyalty_level_id { get; set; }
        public DateTime partner_expired_date { get; set; }
        public bool auto_paid_by_cron { get; set; }
        public int message_main_attachment_id { get; set; }
        public DateTime date_last_order { get; set; }
        public bool cancel_from_be { get; set; }
        public string moca_payment_ref { get; set; }
        public string cancel_reason { get; set; }
        public bool cancel_duplicate { get; set; }
        public bool pay_draft_order { get; set; }
        public string invoice_name { get; set; }
        public string invoice_vat { get; set; }
        public string invoice_address { get; set; }
        public string invoice_email { get; set; }
        public string invoice_contact { get; set; }
        public string invoice_note { get; set; }
        public bool invoice_request { get; set; }
        public string zalo_payment_ref { get; set; }
        public string mobile_receiver_info { get; set; }
        public string partner_insert_type { get; set; }
        public bool? order_in_call_center { get; set; }
        public int? session_callcenter_id { get; set; }
        public decimal cv_life_redeem { get; set; }
        public decimal cv_life_earn { get; set; }
    }
}
