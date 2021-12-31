using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class Product_Material
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? product_id { get; set; }
        public int? create_uid { get; set; }
        public DateTime? create_date { get; set; }
        public int? write_uid { get; set; }
        public DateTime? write_date { get; set; }
        public int? product_custom_id { get; set; }
        public string option_unavailable_dom { get; set; }
        public bool? available_in_mobile { get; set; }
        public string name_mobile { get; set; }
    }
}
