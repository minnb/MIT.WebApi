using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Pos_Payment
    {
		public int id { get; set; }
		public int pos_order_id { get; set; }
		public decimal amount { get; set; }
		public int payment_method_id { get; set; }
		public DateTime payment_date { get; set; }
		public string card_type { get; set; }
		public string transaction_id { get; set; }
		public DateTime create_date { get; set; }
		public string voucher_code { get; set; }
		public int warehouse_id { get; set; }
		public int user_id { get; set; }
		public int cashier_id { get; set; }
		public DateTime date_order { get; set; }
		public string state_pos { get; set; }
		public int? employee_id { get; set; }
		public string currency_name { get; set; }
		public decimal currency_origin_value { get; set; }
		public decimal exchange_rate { get; set; }
		public string on_account_info { get; set; }
		public string mercury_card_number { get; set; }
		public string mercury_card_brand { get; set; }
		public string mercury_card_owner_name { get; set; }
		public string mercury_ref_no { get; set; }
		public string mercury_record_no { get; set; }
		public string mercury_invoice_no { get; set; }
		public decimal amount_change { get; set; }
		public decimal voucher_max_value { get; set; }
		public int? partner_id { get; set; }
		public int? on_account_partner_id { get; set; }
		public string partner_code { get; set; }
		public string sap_method { get; set; }
	}
}
