using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class Pos_Sale_Type
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool active { get; set; }
        public bool? use_for_call_center { get; set; }
        public int? create_uid { get; set; }
        public DateTime create_date { get; set; }
        public int? write_uid { get; set; }
        public DateTime write_date { get; set; }
        public bool? allow_print_label_first { get; set; }
        public bool? show_original_subtotal { get; set; }
        public string sap_code { get; set; }
    }
}
