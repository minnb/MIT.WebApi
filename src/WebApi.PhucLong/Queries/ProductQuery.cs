using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.PhucLong.API.Queries.POS
{
    public static class ProductQuery
    {
        public static string get_product_product()
        {
            return @"select a.id,a.default_code, a.display_name, b.tax_id
                    from product_product a
                    left join product_taxes_rel b on a.id = b.prod_id;";
        }
    }
}
