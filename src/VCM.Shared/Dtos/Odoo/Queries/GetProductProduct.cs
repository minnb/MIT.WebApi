using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.Odoo.Queries
{
    public class GetProductProduct
    {
        public int id { get; set; }
        public string default_code { get; set; }
        public string display_name { get; set; }
        public int tax_id { get; set; }
    }
}
