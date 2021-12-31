using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class Product_Material_Line
    {
        public int id { get; set; }
        public int sequence { get; set; }
        public int product_id { get; set; }
        public int? product_uom_id { get; set; }
        public int product_material_id { get; set; }
        public decimal product_qty { get; set; }
        public int? create_uid { get; set; }
        public DateTime create_date { get; set; }
        public int? write_uid { get; set; }
        public DateTime write_date { get; set; }
    }
}
