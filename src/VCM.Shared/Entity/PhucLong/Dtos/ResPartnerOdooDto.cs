using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class ResPartnerOdooDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public bool? active { get; set; }
        public bool? employee { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public int? commercial_partner_id { get; set; }
        public string gender { get; set; }
        public string marital { get; set; }
        public DateTime birthday { get; set; }
        public int total_point_act { get; set; }
        public int current_point_act { get; set; }
        public DateTime expired_date { get; set; }
        public int? loyalty_level_id { get; set; }
        public string level_name { get; set; }
    }
}
