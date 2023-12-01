using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VCM.PhucLong.API.Database;
using VCM.Shared.API.PLG;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Entity.PhucLong.Dtos;
using VCM.Shared.Enums;
using WebApi.PhucLong.Models;

namespace WebApi.PhucLong.Services
{
    public class CrmService : ICrmService
    {
        private readonly ILogger<CrmService> _logger;
        private readonly DapperOdooContext _context;
        private readonly IRedisService _redisService;
        private readonly IConfiguration _configuration;
        public CrmService
            (
                ILogger<CrmService> logger,
                DapperOdooContext context,
                IRedisService redisService,
                IConfiguration configuration
            )
        {
            _logger = logger;
            _context = context;
            _redisService = redisService;
            _configuration = configuration;
        }

        public async Task<ResponseVoucherCheckOdoo> CheckVoucherOdoo(RequestVoucherCheckOdoo request)
        {
            ResponseVoucherCheckOdoo result = new ResponseVoucherCheckOdoo();
            int statusCode = 0;
            try
            {
                bool isEmployee = request.ISEMPLOYEE == "1" ? true : false;
                using var conn = _context.CreateConnection(1);
                conn.Open();
                string queryVoucher = @"select a.id, a.ean, a.type, a.state, a.publish_date, a.effective_date_from, a.effective_date_to, a.usage_limits, a.used_count, a.employee_id,
	                                    b.name discount_type, b.modify_type, b.start_date_active, b.end_date_active, b.discount_value,
		                                a.pos_order_id, a.apply_for_employee, a.order_reference, a.date_used, b.value_from, b.value_to, b.amount_discount_limit
                                        from public.crm_voucher_info a
                                        inner join sale_promo_lines b on a.promotion_line_id = b.id
                                        where a.type ='coupon' and a.ean like '" + request.SERIALNUMBER + "%' AND apply_for_employee = " + isEmployee + " order by a.publish_date desc limit(1);";
                
                var data = await conn.QueryAsync<select_crm_voucher_info>(queryVoucher).ConfigureAwait(false);
                var voucherData = data.FirstOrDefault();
                CpnVchBOMHeaderDto coupon = new CpnVchBOMHeaderDto();
                var mapItemNo = await _redisService.GetCpnVchBOMHeaderAsync().ConfigureAwait(false);

                CpnVchBOMLineDto lineItemNo = new CpnVchBOMLineDto();
                var mapLineItemNo = await _redisService.GetCpnVchBOMLineAsync().ConfigureAwait(false);

                if (voucherData != null)
                {
                    _logger.LogWarning("===> voucherData: " +JsonConvert.SerializeObject(voucherData));
                    if (mapItemNo.Count > 0)
                    {
                        //var mapItemNo = JsonConvert.DeserializeObject<List<CpnVchBOMHeaderDto>>(jsData).ToList();
                        int discount_type = voucherData.modify_type == "disc_percent" ? 1 : (voucherData.modify_type == "fix_value" || voucherData.modify_type == "disc_value" ? 2 : 999);
                        if(request.ISEMPLOYEE == "1")
                        {
                            coupon = mapItemNo.Where(x => x.ArticleType == "ZCPN" && x.DiscountType == discount_type && x.DiscountValue == voucherData.discount_value && x.CpnVchType == "EMP").FirstOrDefault();
                        }
                        else if(voucherData.amount_discount_limit == 0 && voucherData.value_from >= 1)
                        {
                            //value 20K
                            coupon = mapItemNo.Where(x => x.ArticleType == "ZCPN" && x.DiscountType == discount_type && x.DiscountValue == voucherData.discount_value && x.CpnVchType == "CUS" && x.MinAmount == voucherData.value_from).FirstOrDefault();
                        }
                        else if (voucherData.amount_discount_limit > 0 && discount_type == 1 &&  voucherData.value_from == 1)
                        {
                            //% - valueMax = 250K
                            coupon = mapItemNo.Where(x => x.ArticleType == "ZCPN" && x.DiscountType == discount_type && x.DiscountValue == voucherData.discount_value && x.CpnVchType == "CUS" && x.ValueOfVoucher == voucherData.amount_discount_limit).FirstOrDefault();
                        }
                        else if (voucherData.amount_discount_limit > 0 && discount_type == 1 && voucherData.value_from == 1)
                        {
                            //% - không limit
                            coupon = mapItemNo.Where(x => x.ArticleType == "ZCPN" && x.DiscountType == discount_type && x.DiscountValue == voucherData.discount_value && x.CpnVchType == "CUS").FirstOrDefault();
                        }

                        if (coupon == null)
                        {
                            result.STATUSCODE = VoucherStatusEnum.ERROR.ToString();
                            result.STATUSMESSAGE = "Không tìm thấy CpnVchBOMHeader";
                        }
                        else
                        {
                            result.MATERIALNUMBER = coupon.ItemNo;

                            DateTime today = DateTime.Now.Date;
                            if (today >= voucherData.effective_date_from && today <= voucherData.effective_date_to && voucherData.state != "Close")
                            {
                                statusCode = 1;
                                result.STATUSCODE = CheckStatusVoucher(voucherData.state);
                                result.STATUSMESSAGE = coupon.ItemName;
                            }
                            else
                            {
                                statusCode = 0;
                                result.STATUSMESSAGE = request.ISEMPLOYEE == "1" ?"Coupon chưa gia hạn sử dụng" : "Coupon đã sử dụng";
                                result.STATUSCODE = VoucherStatusEnum.ERROR.ToString();
                            }
                        }
                    }
                    else
                    {
                        result.STATUSCODE = VoucherStatusEnum.ERROR.ToString();
                        result.STATUSMESSAGE = "Không tìm thấy CpnVchBOMHeader";
                    }
                    result.SERIALNUMBER = voucherData.ean.ToString();
                    result.FROMDATE = voucherData.effective_date_from.ToString("yyyyMMdd");
                    result.TODATE = voucherData.effective_date_to.ToString("yyyyMMdd");
                    result.VALUE = voucherData.discount_value.ToString();
                    result.REMAINAMOUNT = (voucherData.usage_limits - voucherData.used_count).ToString();
                    result.SALESDATE = voucherData.date_used.ToString("yyyyMMdd");
                    result.SALESTRANS = voucherData.pos_order_id.ToString();
                    result.REDEEMTRANS = voucherData.pos_order_id.ToString();
                    result.REDEEMDATE = voucherData.date_used.ToString("yyyyMMdd");
                    result.REDEEMPLANT = "";
                    result.ISEMPLOYEE = voucherData.apply_for_employee == true ? "X" : "";
                }
                else
                {
                    voucherData = null;
                    string otherCoupon = @"select a.id, a.ean, a.type, a.state, a.publish_date, a.effective_date_from, a.effective_date_to, a.usage_limits, a.used_count, a.employee_id,
	                                    t.sap_code discount_type, b.modify_type, b.start_date_active, b.end_date_active, b.discount_value,
		                                a.pos_order_id, a.apply_for_employee, a.order_reference, a.date_used, b.value_from, b.value_to, b.amount_discount_limit
                                        from public.crm_voucher_info a
                                        inner join crm_voucher_publish p on p.id = a.publish_id
                                        inner join sale_promo_header h on h.id = p.promotion_header_id
                                        inner join sale_promo_lines b on b.discount_id = h.id
                                        inner join product_template t on t.id = b.product_tmpl_id
                                        where a.apply_for_employee = false AND a.type ='coupon' and a.ean like '" + request.SERIALNUMBER + "%' order by a.publish_date desc limit(1);";
                    var dataOther = await conn.QueryAsync<select_crm_voucher_info>(otherCoupon).ConfigureAwait(false);
                    voucherData = dataOther.FirstOrDefault();
                    if(voucherData != null)
                    {
                        //var mapItemNo = JsonConvert.DeserializeObject<List<CpnVchBOMHeaderDto>>(jsData).ToList();
                        int discount_type = voucherData.modify_type == "disc_percent" ? 1 : (voucherData.modify_type == "fix_value" || voucherData.modify_type == "disc_value" ? 2 : 999);
                        if (request.ISEMPLOYEE == "1")
                        {
                            coupon = mapItemNo.Where(x => x.ArticleType == "ZCPN" && x.DiscountType == discount_type && x.DiscountValue == voucherData.discount_value && x.CpnVchType == "EMP").FirstOrDefault();
                        }
                        else if (voucherData.amount_discount_limit == 0 && voucherData.value_from >= 1)
                        {
                            //value 20K
                            coupon = mapItemNo.Where(x => x.ArticleType == "ZCPN" && x.DiscountType == discount_type && x.DiscountValue == voucherData.discount_value && x.CpnVchType == "CUS" && x.MinAmount == voucherData.value_from).FirstOrDefault();
                        }
                        else if (voucherData.amount_discount_limit > 0 && discount_type == 1 && voucherData.value_from == 1)
                        {
                            //% - valueMax = 250K
                            coupon = mapItemNo.Where(x => x.ArticleType == "ZCPN" && x.DiscountType == discount_type && x.DiscountValue == voucherData.discount_value && x.CpnVchType == "CUS" && x.ValueOfVoucher == voucherData.amount_discount_limit).FirstOrDefault();
                        }
                        else if (voucherData.amount_discount_limit > 0 && discount_type == 1 && voucherData.value_from == 1)
                        {
                            //% - không limit
                            coupon = mapItemNo.Where(x => x.ArticleType == "ZCPN" && x.DiscountType == discount_type && x.DiscountValue == voucherData.discount_value && x.CpnVchType == "CUS").FirstOrDefault();
                        }

                        if (coupon == null)
                        {
                            result.STATUSCODE = VoucherStatusEnum.ERROR.ToString();
                            result.STATUSMESSAGE = "Không tìm thấy CpnVchBOMHeader";
                        }
                        else
                        {
                            result.MATERIALNUMBER = coupon.ItemNo;
                            var checkItemNo = mapLineItemNo.Where(x => x.ItemNo == coupon.ItemNo && x.LineItemNo == voucherData.discount_type).FirstOrDefault();
                            if(checkItemNo != null)
                            {
                                DateTime today = DateTime.Now.Date;
                                if (today >= voucherData.effective_date_from && today <= voucherData.effective_date_to && voucherData.state != "Close")
                                {
                                    statusCode = 1;
                                    result.STATUSCODE = CheckStatusVoucher(voucherData.state);
                                    result.STATUSMESSAGE = coupon.ItemName;
                                }
                                else
                                {
                                    statusCode = 0;
                                    result.STATUSMESSAGE = request.ISEMPLOYEE == "1" ? "Coupon chưa gia hạn sử dụng" : "Coupon đã sử dụng";
                                    result.STATUSCODE = VoucherStatusEnum.ERROR.ToString();
                                }
                            }
                            else
                            {
                                result.STATUSCODE = VoucherStatusEnum.ERROR.ToString();
                                result.STATUSMESSAGE = "Không tìm thấy " + voucherData.discount_type + " trong table CpnVchBOMLine";
                            }
                        }
                        result.SERIALNUMBER = voucherData.ean.ToString();
                        result.FROMDATE = voucherData.effective_date_from.ToString("yyyyMMdd");
                        result.TODATE = voucherData.effective_date_to.ToString("yyyyMMdd");
                        result.VALUE = voucherData.discount_value.ToString();
                        result.REMAINAMOUNT = (voucherData.usage_limits - voucherData.used_count).ToString();
                        result.SALESDATE = voucherData.date_used.ToString("yyyyMMdd");
                        result.SALESTRANS = voucherData.pos_order_id.ToString();
                        result.REDEEMTRANS = voucherData.pos_order_id.ToString();
                        result.REDEEMDATE = voucherData.date_used.ToString("yyyyMMdd");
                        result.REDEEMPLANT = "";
                        result.ISEMPLOYEE = voucherData.apply_for_employee == true ? "X" : "";
                    }
                    else
                    {
                        result.STATUSCODE = VoucherStatusEnum.ERROR.ToString();
                        result.STATUSMESSAGE = "Không tìm thấy CpnVchBOMHeader";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===>CheckVoucherOdoo Exception: " + ex.Message.ToString());
                result.STATUSCODE = VoucherUpdateStatusEnum.NotExist.ToString();
                result.STATUSMESSAGE = "Lỗi Exception ";
            }
            result.STATUSMESSAGECODE = statusCode.ToString();
            _logger.LogWarning("===> result: " + JsonConvert.SerializeObject(result));
            return result; 
        }
        public async Task<ResponseVoucherRedeemOdoo> VoucherRedeemOdoo(RequestVoucherRedeemOdoo request)
        {
            ResponseVoucherRedeemOdoo result = new ResponseVoucherRedeemOdoo
            {
                SERIALNUMBER = request.SeriNo,
                REDEEMPLANT = request.RedeemPlant.ToString(),
                MATERIALNUMBER = ""
            };
            _logger.LogWarning("===> request voucherRedeem: " + JsonConvert.SerializeObject(request));
            try
            {
                using var conn = _context.CreateConnection(1);
                conn.Open();
                string queryVoucher = @"select a.id, a.ean, a.type, a.state, a.publish_date, a.effective_date_from, a.effective_date_to, a.usage_limits, a.used_count, a.employee_id,
	                                    b.name discount_type, b.modify_type, b.start_date_active, b.end_date_active, b.discount_value,
		                                a.pos_order_id, a.apply_for_employee, a.order_reference, a.date_used
                                        from public.crm_voucher_info a
                                        left join sale_promo_lines b on a.promotion_line_id = b.id
                                        where a.type ='coupon' and a.ean = '" + request.SeriNo + "';";

                var data = await conn.QueryAsync<select_crm_voucher_info>(queryVoucher).ConfigureAwait(false);
                var voucherData = data.FirstOrDefault();
                if (voucherData != null)
                {
                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        var today = DateTime.Now.Date;
                        if (voucherData.used_count < voucherData.usage_limits && voucherData.state == "Create" && voucherData.effective_date_from.Date <= today && voucherData.effective_date_to.Date >= today)
                        {
                            voucherData.used_count += int.Parse(request.RedeemAmount);
                            if(int.Parse(request.RedeemAmount) > 1000)
                            {
                                voucherData.used_count = voucherData.usage_limits;
                            }

                            if (voucherData.apply_for_employee && !string.IsNullOrEmpty(request.RedeemAmount))
                            {
                                //voucherData.used_count += int.Parse(request.RedeemAmount);
                                voucherData.state = "Create";
                            }
                            else if(!voucherData.apply_for_employee && !string.IsNullOrEmpty(request.RedeemAmount) && int.Parse(request.RedeemAmount) > 1000)
                            {
                                //voucherData.used_count = voucherData.usage_limits;
                                voucherData.state = "Close";
                            }

                            voucherData.order_reference = request.RedeemTrans.ToString();
                            voucherData.date_used = DateTime.Now.Date;

                            var querySold = @"UPDATE public.crm_voucher_info SET state = @state, order_reference = @order_reference, used_count = @used_count, write_date = now(), date_used = @date_used
                                                 WHERE ean = @ean and state = 'Create' and usage_limits >= @used_count;";

                            await conn.ExecuteAsync(querySold, voucherData, transaction);
                            transaction.Commit();
                            result.STATUSCODE = VoucherStatusEnum.REDE.ToString();
                            result.STATUSMESSAGECODE = "1";
                            result.STATUSMESSAGE = "So serial " + request.SeriNo + " cap nhat Redeem thanh cong";
                            result.REMAINAMOUNT = (voucherData.usage_limits - voucherData.used_count).ToString();
                        }
                        else if(voucherData.used_count == voucherData.usage_limits)
                        {
                            result.STATUSCODE = VoucherStatusEnum.ERROR.ToString();
                            result.STATUSMESSAGECODE = "0";
                            result.STATUSMESSAGE = "Đã sử dụng " + voucherData.used_count.ToString() + "/" + voucherData.usage_limits.ToString();
                        }
                        else if (voucherData.effective_date_from.Date >= today || voucherData.effective_date_to.Date <= today)
                        {
                            result.STATUSCODE = VoucherStatusEnum.ERROR.ToString();
                            result.STATUSMESSAGECODE = "0";
                            result.STATUSMESSAGE = "Coupon hết hạn sử dụng";
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.STATUSCODE = VoucherUpdateStatusEnum.Errors.ToString();
                        _logger.LogWarning(ex.Message.ToString());
                    }

                }
                else
                {
                    result.STATUSCODE = VoucherUpdateStatusEnum.NotExist.ToString();
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message.ToString());
                result.STATUSCODE = VoucherUpdateStatusEnum.Errors.ToString();
            }
            _logger.LogWarning("===> response voucherRedeem: " + JsonConvert.SerializeObject(result));
            return result;
        }
        public async Task<VoucherInfoDto> UpdateStatusVoucher(RequestVoucherPLG request)
        {
            try
            {
                using var conn = _context.CreateConnection(1);
                conn.Open();
                string queryVoucher = @"SELECT  id, ean as serial_number, publish_date, publish_id, state, 
		                                        (case when effective_date_from is null then '9999-01-01' else effective_date_from end) as effective_date_from,
		                                        (case when effective_date_to is null then '9999-01-01' else effective_date_to end) as effective_date_to,
		                                        voucher_amount, order_reference, 'api' used_on, type,
		                                        (case when date_used is null then '1990-01-01' else date_used end) as date_used
                                        FROM public.crm_voucher_info
                                        where ean = '" + request.serial_number + "';";

                var data = await conn.QueryAsync<VoucherInfoDto>(queryVoucher).ConfigureAwait(false);
                var voucherData = data.FirstOrDefault();
                if (voucherData != null)
                {
                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        if (voucherData.state == VoucherStateEnumPLG.Create.ToString())
                        {
                            DateTime from_date = DateTime.ParseExact(request.effective_date_from, "yyyyMMdd", CultureInfo.InvariantCulture);
                            DateTime to_date = DateTime.ParseExact(request.effective_date_to, "yyyyMMdd", CultureInfo.InvariantCulture);
                            DateTime today = DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture);

                            if (request.status.ToUpper() == VoucherStatusEnum.SOLD.ToString())
                            {
                                if (voucherData.effective_date_from.Year == 2050)
                                {
                                    
                                    voucherData.effective_date_from = from_date;
                                    voucherData.effective_date_to = to_date;
                                    voucherData.serial_number = request.serial_number;
                                    voucherData.state = VoucherStateEnumPLG.Create.ToString();
                                    voucherData.update_status = VoucherUpdateStatusEnum.Success.ToString();

                                    var querySold = @"UPDATE public.crm_voucher_info SET effective_date_from = @effective_date_from, effective_date_to = @effective_date_to, write_date = now()
                                                 WHERE id = @id and ean = @serial_number and state = 'Create';";

                                    await conn.ExecuteAsync(querySold, voucherData, transaction);

                                    _logger.LogWarning("Successfully: " + JsonConvert.SerializeObject(request));
                                }
                                else
                                {
                                    voucherData.update_status = VoucherUpdateStatusEnum.Activated.ToString();
                                }
                            }
                            else if (request.status.ToUpper() == VoucherStatusEnum.REDE.ToString() && voucherData.effective_date_from != null && voucherData.effective_date_to != null)
                            {
                                if (today <= voucherData.effective_date_to && today >= voucherData.effective_date_from)
                                {
                                    voucherData.state = VoucherStateEnumPLG.Close.ToString();
                                    voucherData.update_status = VoucherUpdateStatusEnum.Success.ToString();
                                    var queryRedeem = @"UPDATE public.crm_voucher_info SET state = @state WHERE id = @id and ean = @serial_number;";
                                    await conn.ExecuteAsync(queryRedeem, voucherData, transaction);
                                }
                                else
                                {
                                    voucherData.update_status = VoucherUpdateStatusEnum.OutOfDate.ToString();
                                }
                            }
                        }
                        else
                        {
                            voucherData.update_status = VoucherUpdateStatusEnum.UsedOrExpired.ToString();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        voucherData.update_status = VoucherUpdateStatusEnum.Errors.ToString();
                        _logger.LogError(ex.Message.ToString());
                    }

                }
                else
                {
                    voucherData.update_status = VoucherUpdateStatusEnum.NotExist.ToString();
                }
                return voucherData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                return null;
            }
        }
        private string CheckStatusVoucher(string status)
        {
            return status switch
            {
                "Create" => VoucherStatusEnum.SOLD.ToString(),
                "Close" => VoucherStatusEnum.REDE.ToString(),
                _ => VoucherStatusEnum.AVAI.ToString(),
            };
        }
    }
}
