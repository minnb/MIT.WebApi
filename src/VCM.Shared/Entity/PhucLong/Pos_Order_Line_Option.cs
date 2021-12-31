using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class Pos_Order_Line_Option
    {
        public int id { get; set; }
        public int line_id { get; set; }
        public int option_id { get; set; }
        public string option_type { get; set; }
        public string name { get; set; }
        public int product_id { get; set; }
        public int product_uom_id { get; set; }
        public int product_material_id { get; set; }
        public decimal product_qty { get; set; }
    }
}
