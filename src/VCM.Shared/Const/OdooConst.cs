using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Const
{
    public static class OdooConst
    {
        public static string MappingMD_Odoo(int id)
        {
            Dictionary<int, string> openWith = new Dictionary<int, string>()
            {
                //product
                { 10, "product_product" },
                { 11, "product_category" },
                { 12, "product_template" },
                { 13, "product_taxes_rel" },
                { 14, "uom_uom" },
                { 15, "product_material" },
                { 16, "pos_sale_type" },
                //sales
                { 20, "pos_config" },
                { 21, "payment_method" },
                //stock
                { 30, "stock_location" },
                { 31, "stock_warehouse" },
                //promotion
                { 40, "sale_promo_header" },
                { 41, "sale_promo_lines" },
                //partner
                { 50, "res_partner" },
            };
            return openWith[id];

        }
    }
}
