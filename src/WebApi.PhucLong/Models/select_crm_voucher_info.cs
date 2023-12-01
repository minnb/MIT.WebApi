using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.PhucLong.Models
{
    public class select_crm_voucher_info
    {
        public int id { get; set; }
        public string ean { get;set;}
        public string type { get; set; }
        public string state { get; set; }
        public DateTime publish_date { get; set; }
        public DateTime effective_date_from { get; set; }
        public DateTime effective_date_to { get; set; }
        public int usage_limits { get; set; }
        public int used_count { get; set; }
        public int employee_id { get; set; }
        public string discount_type { get; set; }
        public string modify_type { get; set; }
        public DateTime start_date_active { get; set; }
        public DateTime end_date_active { get; set; }
        public decimal discount_value { get; set; }
        public DateTime date_used { get; set; }
        public int pos_order_id { get; set; }
        public bool apply_for_employee { get; set; }
        public string order_reference { get; set; }
        public decimal value_from { get; set; }
        public decimal value_to { get; set; }
        public decimal amount_discount_limit { get; set; }
    }
}
