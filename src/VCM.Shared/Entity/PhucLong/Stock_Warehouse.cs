using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Stock_Warehouse
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool? active { get; set; }
        public int? company_id { get; set; }
        public int? partner_id { get; set; }
        public int? view_location_id { get; set; }
        public int? lot_stock_id { get; set; }
        public string code { get; set; }
        public string reception_steps { get; set; }
        public string delivery_steps { get; set; }
        public int? wh_input_stock_loc_id { get; set; }
        public int? wh_qc_stock_loc_id { get; set; }
        public int? wh_output_stock_loc_id { get; set; }
        public int? wh_pack_stock_loc_id { get; set; }
        public int? mto_pull_id { get; set; }
        public int? pick_type_id { get; set; }
        public int? pack_type_id { get; set; }
        public int? out_type_id { get; set; }
        public int? in_type_id { get; set; }
        public int? int_type_id { get; set; }
        public int? crossdock_route_id { get; set; }
        public int? reception_route_id { get; set; }
        public int? delivery_route_id { get; set; }
        public int? sequence { get; set; }
        public int? create_uid { get; set; }
        public DateTime create_date { get; set; }
        public int? write_uid { get; set; }
        public DateTime write_date { get; set; }
        public bool? buy_to_resupply { get; set; }
        public int? buy_pull_id { get; set; }
        public int? message_main_attachment_id { get; set; }
        public bool? man_lots { get; set; }
        public int? return_customer_type_id { get; set; }
        public int? return_supplier_type_id { get; set; }
        public int? consumption_type_id { get; set; }
        public int? scrap_type_id { get; set; }
        public int? adj_type_id { get; set; }
        public int? transit_out_type_id { get; set; }
        public int? transit_in_type_id { get; set; }
        public int? account_analytic_id { get; set; }
        public int? company_account_analytic_id { get; set; }
        public string contact_address { get; set; }
        public int? receipt_from_customer_route_id { get; set; }
        public string receipt_from_customer_steps { get; set; }
        public string pos_type_id { get; set; }
        public int? region_stock_id { get; set; }
        public bool? resupply_all_wh { get; set; }
    }
}
