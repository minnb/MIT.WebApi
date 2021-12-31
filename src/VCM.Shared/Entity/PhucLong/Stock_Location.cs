using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Stock_Location
    {
        public int id { get; set; }
        public string parent_path { get; set; }
        public string name { get; set; }
        public string complete_name { get; set; }
        public bool active { get; set; }
        public string usage { get; set; }
        public int? location_id { get; set; }
        public string comment { get; set; }
        public int posx { get; set; }
        public int posy { get; set; }
        public int posz { get; set; }
        public int? company_id { get; set; }
        public bool scrap_location { get; set; }
        public bool? return_location { get; set; }
        public int? removal_strategy_id { get; set; }
        public string barcode { get; set; }
        public int? valuation_in_account_id { get; set; }
        public int? valuation_out_account_id { get; set; }
        public int? warehouse_id { get; set; }
        public bool? consignment_location { get; set; }
    }
}
