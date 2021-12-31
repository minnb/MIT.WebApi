using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Payment_Method
    {
        public int id { get; set; }
        public string name { get; set; }
        public int receivable_account_id { get; set; }
        public bool? is_cash_count { get; set; }
        public int cash_journal_id { get; set; }
        public bool? split_transactions { get; set; }
        public int? company_id { get; set; }
        public string use_payment_terminal { get; set; }
        public bool use_for_loyalty { get; set; }
        public bool use_for_voucher { get; set; }
        public string use_for { get; set; }
        public int? pos_mercury_config_id { get; set; }
        public int? sequence { get; set; }
        public bool? momo_journal { get; set; }
        public int? giftcode_api { get; set; }
        public bool? moca_journal { get; set; }
        public bool? zalo_journal { get; set; }
        public string partner_code { get; set; }
        public string sap_method { get; set; }
    }
}
