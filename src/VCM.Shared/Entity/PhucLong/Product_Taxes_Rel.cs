using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Product_Taxes_Rel
    {
        public string item_no { get; set; }
        public string item_name { get; set; }
        public string uom { get; set; }
        public string vat_group { get; set; }
        public int prod_id { get; set; }
        public int tax_id { get; set; }
        public decimal vat_percent { get; set; }
        public string description { get; set; }
    }
}
