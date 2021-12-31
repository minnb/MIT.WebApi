using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Odoo.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using VCM.Common.Helpers;
using VCM.Shared.Const;

namespace PhucLong.Interface.Odoo.AppService
{
    public class MasterAppService
    {
        private IConfiguration _config;
        private readonly ConndbNpgsql _dbOdoo;
        public MasterAppService
            (
             IConfiguration config
            )
        {
            _config = config;
            _dbOdoo = new ConndbNpgsql(_config);
        }

        public void GetMasterDataOdoo(string connectString, string _pathLocalMaster)
        {
            try
            {

                using IDbConnection conn = _dbOdoo.GetConndbNpgsql(connectString);
                conn.Open();
                CreateFileMasterOdoo(10, conn.Query(GetQuery(10)).ToList(), _pathLocalMaster);
                CreateFileMasterOdoo(11, conn.Query(GetQuery(11)).ToList(), _pathLocalMaster);
                CreateFileMasterOdoo(12, conn.Query(GetQuery(12)).ToList(), _pathLocalMaster);
                CreateFileMasterOdoo(13, conn.Query(GetQuery(13)).ToList(), _pathLocalMaster);
                CreateFileMasterOdoo(14, conn.Query(GetQuery(14)).ToList(), _pathLocalMaster);
                CreateFileMasterOdoo(15, conn.Query(GetQuery(15)).ToList(), _pathLocalMaster);
                CreateFileMasterOdoo(16, conn.Query(GetQuery(16)).ToList(), _pathLocalMaster);

                CreateFileMasterOdoo(20, conn.Query(GetQuery(20)).ToList(), _pathLocalMaster);
                CreateFileMasterOdoo(21, conn.Query(GetQuery(21)).ToList(), _pathLocalMaster);
                CreateFileMasterOdoo(30, conn.Query(GetQuery(30)).ToList(), _pathLocalMaster);
                CreateFileMasterOdoo(31, conn.Query(GetQuery(31)).ToList(), _pathLocalMaster);
                
                CreateFileMasterOdoo(40, conn.Query(GetQuery(31)).ToList(), _pathLocalMaster);

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("GetMasterDataOdoo Exception: " + ex.Message.ToString());
            }
        }

        private string GetQuery(int typeMD)
        {
            string query = string.Empty;
            switch (typeMD)
            {
                case 10: //product_product
                    query = @"SELECT id, default_code, active, product_tmpl_id, barcode,combination_indices,weight,can_image_variant_1024_be_zoomed,message_main_attachment_id,display_name,parent_categ_id,category_lv1,category_lv2,category_lv3,volume 
                            FROM public.product_product;";
                    break;
                case 11: //product_category
                    query = @"SELECT id, parent_path, name, complete_name, parent_id, create_uid, create_date, write_uid, write_date, removal_strategy_id, sequence, code, type, level, fnb_type
	                        FROM public.product_category;";
                    break;
                case 12: //product_template
                    query = @"SELECT id, name, sequence, description, description_purchase, description_sale, type, rental, categ_id, list_price, volume, weight, sale_ok, purchase_ok, uom_id, uom_po_id, company_id, active, color, default_code, 
                            can_image_1024_be_zoomed, has_configurable_attributes, message_main_attachment_id, create_uid, create_date, write_uid, write_date, sale_delay, tracking, description_picking, description_pickingout, description_pickingin, 
                            purchase_method, purchase_line_warn, purchase_line_warn_msg, service_type, sale_line_warn, sale_line_warn_msg, expense_policy, invoice_policy, service_to_purchase, life_time, use_time, removal_time, alert_time, 
                            parent_categ_id, category_lv1, category_lv2, category_lv3, categ_lv1_id, removal_strategy_id, removal_method, available_in_pos, to_weight, pos_categ_id, pos_sequence, short_name, size_id, ref_code, lock_item_method, 
                            fnb_type, lid_id, eng_name, cup_type, parent_code, provided_by, spoon_id, is_cashless, effective_day, update_coupon_expiration, sap_code, sap_uom
	                        FROM public.product_template;";
                    break;
                case 13: //product_taxes_rel
                    query = @"SELECT prod_id, tax_id FROM public.product_taxes_rel;";
                    break;
                case 14: //uom_uom
                    query = @"SELECT id, name, category_id, factor, rounding, active, uom_type, measure_type, create_uid, create_date, write_uid, write_date 
                            FROM public.uom_uom;";
                    break;

                case 15: //product_material
                    query = @"select a.id, a.name, a.product_custom_id, b.id as product_material_id, b.product_id, b.product_uom_id, b.product_qty
                                from product_material a
                                inner join product_material_line b on a.id = b.product_material_id;";
                    break;

                case 16: //pos_sale_type
                    query = @"SELECT id, name, description, active, use_for_call_center, create_uid, create_date, write_uid, write_date, allow_print_label_first, show_original_subtotal, sap_code
                               FROM public.pos_sale_type;";
                    break;
                
                case 20: //pos_config
                    query = @"SELECT id, name, picking_type_id, journal_id, invoice_journal_id, iface_cashdrawer, iface_electronic_scale, iface_vkeyboard, iface_customer_facing_display, iface_print_via_proxy, iface_scan_via_proxy, 
                                                            iface_big_scrollbars, iface_print_auto, iface_print_skip_screen, iface_precompute_cash, iface_tax_included, iface_start_categ_id, iface_display_categ_images, restrict_price_control, cash_control, receipt_header, 
                                                            receipt_footer, proxy_ip, active, uuid, sequence_id, sequence_line_id, pricelist_id, company_id, barcode_nomenclature_id, group_pos_manager_id, group_pos_user_id, iface_tipproduct, tip_product_id, 
                                                            default_fiscal_position_id, default_cashbox_id, use_pricelist, tax_regime, tax_regime_selection, start_category, limit_categories, module_account, module_pos_restaurant, module_pos_discount, module_pos_loyalty, module_pos_mercury, module_pos_reprint, is_posbox, 
                                                            is_header_or_footer, module_pos_hr, amount_authorized_diff, other_devices, create_uid, create_date, write_uid, write_date, module_pos_iot, epson_printer_ip, crm_team_id, stock_location_id, use_opening_balance, use_closing_balance, warehouse_id, use_pos_saleman, 
                                                            permission_destroy_order, permission_destroy_line, use_manual_discount, permission_discount, printer_name, iface_pos_return_product, maximum_date, sale_type_default_id, is_dollar_pos, use_barcode_scanner_to_open_session, use_multi_printer, use_external_display, 
                                                            order_break_timeout, is_sandbox_env, max_order_to_create, update_cashier_to_session, use_for_mobile, use_replacement_printer, printer_ip, posid
	                                                        FROM public.pos_config;";
                    break;
                case 21: //payment_method
                    query = @"SELECT id, name,receivable_account_id,is_cash_count,cash_journal_id,split_transactions,company_id,use_payment_terminal,use_for_loyalty,use_for_voucher,use_for,pos_mercury_config_id,sequence,momo_journal,
                            giftcode_api,moca_journal,zalo_journal, partner_code, sap_method
                                                        FROM public.pos_payment_method;";
                    break;
                case 30: //stock_location
                    query = @"SELECT id, parent_path, name, complete_name, active, usage, location_id, comment, posx, posy, posz, company_id, scrap_location, return_location, 
		                             removal_strategy_id, barcode, valuation_in_account_id, valuation_out_account_id, warehouse_id, consignment_location
                            FROM public.stock_location;";
                    break;
                case 31: //stock_warehouse
                    query = @"SELECT id, name, active, company_id, partner_id, view_location_id, lot_stock_id, code, reception_steps, delivery_steps, wh_input_stock_loc_id, wh_qc_stock_loc_id, wh_output_stock_loc_id, wh_pack_stock_loc_id,
                            mto_pull_id, pick_type_id, pack_type_id, out_type_id, in_type_id, int_type_id, crossdock_route_id, reception_route_id, delivery_route_id, sequence, create_uid, create_date, write_uid, write_date, buy_to_resupply, 
                            buy_pull_id, message_main_attachment_id, man_lots, return_customer_type_id, return_supplier_type_id, consumption_type_id, scrap_type_id, adj_type_id, transit_out_type_id, transit_in_type_id, account_analytic_id, 
                            company_account_analytic_id, contact_address, receipt_from_customer_route_id, receipt_from_customer_steps, pos_type_id, region_stock_id, resupply_all_wh
	                          FROM public.stock_warehouse;";
                    break;
                
                 //Promotion
                case 40: //sale_promo_header
                    query = @"SELECT id, message_main_attachment_id, name, description, active, list_type, use_for_coupon, apply_type, order_type, start_date_active, end_date_active, start_hour, end_hour, first_get_flag, compile_flag, currency_id, 
                                company_id, state, search_product_ean, requested_by, approved_by, create_uid, create_date, write_uid, write_date, pos_payment_method_id
	                            FROM public.sale_promo_header;";
                    break;
            }
            return query;
        }
        private bool CreateFileMasterOdoo(int mdType, List<object> lstData, string _pathLocalMaster)
        {
            string tableName = OdooConst.MappingMD_Odoo(mdType).ToString();
            try
            {
                if (lstData.Count > 0)
                {
                    string fileName = CheckFileMasterOdoo(mdType, _pathLocalMaster);
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    if (FileHelper.CreateFileMaster("", tableName, _pathLocalMaster, JsonConvert.SerializeObject(lstData)))
                    {
                        FileHelper.WriteLogs("===> CreateFileMasterOdoo: " + tableName);
                        Console.WriteLine("CreateFileMasterOdoo: " + tableName);
                        Thread.Sleep(500);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("CreateFileMasterOdoo Exception: " + ex.Message.ToString());
                return false;
            }
        }
        private string CheckFileMasterOdoo(int mdType, string pathLocalMaster)
        {
            List<string> lstFile = FileHelper.GetFileFromDir(pathLocalMaster, "*.txt");
            string fileName = OdooConst.MappingMD_Odoo(mdType).ToString();
            string result = string.Empty;
            foreach (var file in lstFile)
            {
                if (file.ToString().Contains(fileName))
                {
                    result = pathLocalMaster + file;
                    break;
                }
            }
            return result;
        }
    }
}
