using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class StoreInfoOdooDto
    {
        public int store_id { get; set; }
        public string store_name { get; set; }
        public int pos_id { get; set; }
        public string store_no { get; set; }
        public string pos_no { get; set; }
        public string receipt_header { get; set; }
        public string receipt_footer { get; set; }
    }
}
