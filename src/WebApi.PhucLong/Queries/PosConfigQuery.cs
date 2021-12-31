using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.PhucLong.API.Queries.POS
{
    public static class PosConfigQuery
    {
        public static string QueryPosConfig(string pos_name)
        {
            if (!string.IsNullOrEmpty(pos_name))
            {
                return @"SELECT DISTINCT a.id, c.code location_no, a.name pos_no, b.name location_name, a.stock_location_id, a.warehouse_id
                        FROM public.pos_config a
                        INNER JOIN public.stock_location b on a.stock_location_id = b.id AND b.active = true
                        INNER JOIN public.stock_warehouse c on c.id = a.warehouse_id and c.lot_stock_id = a.stock_location_id
                        WHERE a.name = '" + pos_name + "' AND a.active = true and a.stock_location_id is not null;";
            }
            else
            {
                return @"SELECT DISTINCT a.id, c.code location_no, a.name pos_no, b.name location_name, a.stock_location_id, a.warehouse_id
                        FROM public.pos_config a
                        INNER JOIN public.stock_location b on a.stock_location_id = b.id AND b.active = true
                        INNER JOIN public.stock_warehouse c on c.id = a.warehouse_id and c.lot_stock_id = a.stock_location_id
                        WHERE a.active = true and a.stock_location_id is not null;";
            }
           
        }
    }
}
