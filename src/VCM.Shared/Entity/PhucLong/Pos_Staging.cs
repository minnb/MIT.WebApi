using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Pos_Staging
    {
        public long order_id { get; set; }
        public int location_id { get; set; }
        public string pos_reference { get; set; }
        public bool is_payment { get; set; }
        public bool is_display { get; set; }
        public string state { get; set; }
        public string raw_data { get; set; }
        public DateTime crt_date { get; set; }
        public DateTime upd_date { get; set; }
    }
}

