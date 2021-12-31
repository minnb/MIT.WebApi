using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Product_Product
    {
        public int id { get; set; }
        public string default_code { get; set; }
        public bool active { get; set; }
        public int product_tmpl_id { get; set; }
        public string barcode { get; set; }
        public string combination_indices { get; set; }
        public decimal? weight { get; set; }
        public bool? can_image_variant_1024_be_zoomed { get; set; }
        public int? message_main_attachment_id { get; set; }
        public string display_name { get; set; }
        public int? parent_categ_id { get; set; }
        public string category_lv1 { get; set; }
        public string category_lv2 { get; set; }
        public string category_lv3 { get; set; }
        public decimal? volume { get; set; }
    }
}
