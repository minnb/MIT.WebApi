using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Odoo.Queries
{
    public static class ArchivePosStagingQuery
    {
        public static string ArchivePosStaging(string crt_date)
        {
            return @"SELECT order_id, location_id, pos_reference, is_payment, is_display, state, raw_data, crt_date, upd_date
                        FROM public.pos_staging
                        WHERE Cast(crt_date::timestamp AT TIME ZONE 'UTC' as date) <= '" + crt_date  + @"';";
        }
    }
}
