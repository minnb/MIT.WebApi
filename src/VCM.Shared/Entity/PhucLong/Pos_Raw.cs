using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class Pos_Raw
    {
        public int order_id { get; set; }
        public int location_id { get; set; }
        public bool is_sending { get; set; }
        public string raw_data { get; set; }
        public DateTime crt_date { get; set; }
    }
}
