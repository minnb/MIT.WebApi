using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.Odoo.Queries
{
    public class GetPosConfig
    {
        public int id { get; set; }
        public int stock_location_id { get; set; }
        public int warehouse_id { get; set; }
        public string location_no { get; set; }
        public string pos_no { get; set; }
        public string location_name { get; set; }

    }
}
