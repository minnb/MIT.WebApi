using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong.Dtos
{
    public class VoucherInfoDto
    {
        public int id { get; set; }
        public string serial_number { get; set; }
        public string type { get; set; }
        public DateTime publish_date { get; set; }
        public int publish_id { get; set; }
        public string state { get; set; }
        public DateTime effective_date_from { get; set; }
        public DateTime effective_date_to { get; set; }
        public DateTime date_used { get; set; }
        public decimal voucher_amount { get; set; }
        public string order_reference { get; set; }
        public string used_on { get; set; }
        public string update_status { get; set; }
    }
}
