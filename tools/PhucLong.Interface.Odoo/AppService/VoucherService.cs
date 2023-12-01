using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Odoo.Database;
using PhucLong.Interface.Odoo.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using VCM.Common.Helpers;
using VCM.Shared.Entity.PhucLong;
using VCM.Shared.Enums;

namespace PhucLong.Interface.Odoo.AppService
{
    public class VoucherService
    {
        private IConfiguration _config;
        private readonly ConndbNpgsql _dbOdoo;
        private PosRawService posRawService;
        public VoucherService
            (
             IConfiguration config
            )
        {
            _config = config;
            _dbOdoo = new ConndbNpgsql(_config);
            posRawService = new PosRawService(_config);
        }
        public void GetVoucherInfoOdoo(string connectString, string date_order, int _limit, string pathLocal)
        {
            FileHelper.WriteLogs(connectString);
            GetVoucherInfo(connectString, date_order, _limit, pathLocal, VoucherStateEnumPLG.Create.ToString(), (int)RawTypeOdooEnum.VoucherInfoCreate);
            Thread.Sleep(500);
            GetVoucherInfo(connectString, date_order, _limit, pathLocal, VoucherStateEnumPLG.Close.ToString(), (int)RawTypeOdooEnum.VoucherInfoClose);

        }
        private void GetVoucherInfo(string connectString, string date_order, int _limit, string pathLocal, string state, int location_id)
        {
            //Process
            try
            {
                using IDbConnection conn = _dbOdoo.GetConndbNpgsql(connectString);
                conn.Open();
                Console.WriteLine("top " + _limit.ToString() + " processing....");
                string queryVoucher = @"SELECT id, ean, publish_date, company_id, customer_id, publish_id, state, effective_date_from, effective_date_to, promotion_line_id, voucher_amount, type, 
	                                    order_reference, usage_limits, used_count, create_uid, create_date, write_uid, write_date, warehouse_id, apply_for_employee, employee_id, 
	                                    appear_code_id, pos_order_id, date_used, product_coupon_order_ref
                                    FROM public.crm_voucher_info s
                                    WHERE NOT EXISTS (SELECT 1 FROM public.pos_raw p WHERE p.location_id = " + location_id + @" AND s.id = p.order_id) 
                                            AND type = 'voucher' AND state = '" + state + @"'
                                            AND Cast(write_date::timestamp AT TIME ZONE 'UTC' as date) >= '" + date_order + @"';";

                var dataVoucher = conn.Query<Crm_Voucher_Info>(queryVoucher).ToList();
                List<Pos_Raw> trans = new List<Pos_Raw>();
                foreach (var item in dataVoucher)
                {
                    trans.Add(new Pos_Raw()
                    {
                        order_id = item.id,
                        location_id = location_id,
                        is_sending = false,
                        raw_data = JsonConvert.SerializeObject(item)
                    });
                }

                if (trans.Count > 0)
                {
                    string type = string.Empty;
                    if (state == VoucherStateEnumPLG.Create.ToString()) {
                        type = "A";
                    } else {
                        type = "Z";
                    }
                    posRawService.SavePosRawOdoo(conn, trans, "VOUCHER" + "_" + type.ToString(), pathLocal, true);
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("VOUCHER Exception: " + ex.Message.ToString());
            }
        }
    }
}
