using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong.Dtos
{
    public class ProductMaterialOdooDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? product_custom_id { get; set; }
        public int? product_material_id { get; set; }
        public int? product_id { get; set; }
        public int? product_uom_id { get; set; }
        public decimal product_qty { get; set; }
    }
}
