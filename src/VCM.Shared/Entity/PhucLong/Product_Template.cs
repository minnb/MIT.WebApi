using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Product_Template
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? sequence { get; set; }
        public string description { get; set; }
        public string description_purchase { get; set; }
        public string description_sale { get; set; }
        public string type { get; set; }
        public bool? rental { get; set; }
        public int? categ_id { get; set; }
        public decimal list_price { get; set; }
        public decimal volume { get; set; }
        public decimal weight { get; set; }
        public bool? sale_ok { get; set; }
        public bool? purchase_ok { get; set; }
        public int? uom_id { get; set; }
        public int? uom_po_id { get; set; }
        public int? company_id { get; set; }
        public bool active { get; set; }
        public int? color { get; set; }
        public string default_code { get; set; }
        public bool? can_image_1024_be_zoomed { get; set; }
        public bool? has_configurable_attributes { get; set; }
        public int? message_main_attachment_id { get; set; }
        public int? create_uid { get; set; }
        public DateTime create_date { get; set; }
        public int? write_uid { get; set; }
        public DateTime write_date { get; set; }
        public decimal sale_delay { get; set; }
        public string tracking { get; set; }
        public string description_picking { get; set; }
        public string description_pickingout { get; set; }
        public string description_pickingin { get; set; }
        public string purchase_method { get; set; }
        public string purchase_line_warn { get; set; }
        public string purchase_line_warn_msg { get; set; }
        public string service_type { get; set; }
        public string sale_line_warn { get; set; }
        public string sale_line_warn_msg { get; set; }
        public string expense_policy { get; set; }
        public string invoice_policy { get; set; }
        public bool? service_to_purchase { get; set; }
        public int? life_time { get; set; }
        public int? use_time { get; set; }
        public int? removal_time { get; set; }
        public int? alert_time { get; set; }
        public int? parent_categ_id { get; set; }
        public string category_lv1 { get; set; }
        public string category_lv2 { get; set; }
        public string category_lv3 { get; set; }
        public int? categ_lv1_id { get; set; }
        public int? removal_strategy_id { get; set; }
        public string removal_method { get; set; }
        public string available_in_pos { get; set; }
        public bool? to_weight { get; set; }
        public int? pos_categ_id { get; set; }
        public int? pos_sequence { get; set; }
        public string short_name { get; set; }
        public int? size_id { get; set; }
        public string ref_code { get; set; }
        public string lock_item_method { get; set; }
        public string fnb_type { get; set; }
        public int? lid_id { get; set; }
        public string eng_name { get; set; }
        public string cup_type { get; set; }
        public string parent_code { get; set; }
        public string provided_by { get; set; }
        public int? spoon_id { get; set; }
        public bool? is_cashless { get; set; }
        public int? effective_day { get; set; }
        public bool? update_coupon_expiration { get; set; }
        public string sap_code { get; set; }
        public string sap_uom { get; set; }
    }
}
