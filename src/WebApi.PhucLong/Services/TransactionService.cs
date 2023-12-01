using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VCM.Shared.API;
using VCM.Shared.API.PLG;
using VCM.PhucLong.API.Database;
using VCM.PhucLong.API.Queries.POS;
using VCM.Shared.Entity.PhucLong;
using VCM.Shared.Dtos.POS;
using WebApi.PhucLong.Services;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using WebApi.PhucLong.Models;

namespace VCM.PhucLong.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ILogger<TransactionService> _logger;
        private readonly DapperOdooContext _context;
        private readonly IRedisService _redisService;
        private readonly IConfiguration _configuration;
        public TransactionService
            (
                ILogger<TransactionService> logger,
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
        public async Task<ResponseOrderDetail> GetOrderAsync(int set, string order_id, int location_id)
        {
            ResponseOrderDetail responseOrderDetail = new ResponseOrderDetail();
            try
            {
                using var conn = _context.CreateConnection(set);
                conn.Open();

                Pos_Staging result = null;
                string jsData = await _redisService.GetRedisValueAsync(location_id + "." + order_id);

                if (!string.IsNullOrEmpty(jsData))
                {
                    result = JsonConvert.DeserializeObject<Pos_Staging>(jsData);
                }
                else
                {
                    var data = await conn.QueryAsync<Pos_Staging>(@"SELECT order_id, location_id, pos_reference, is_display, is_payment, state, raw_data, crt_date FROM pos_staging WHERE pos_reference = '" + order_id + "';").ConfigureAwait(false);
                    if(data?.FirstOrDefault() != null)
                    {
                        result = data?.FirstOrDefault();
                    }
                    else
                    {
                        var dataOld = await conn.QueryAsync<Pos_Staging>(@"SELECT order_id, location_id, pos_reference, is_display, is_payment, state, raw_data, crt_date FROM pos_staging_history WHERE pos_reference = '" + order_id + "';").ConfigureAwait(false);
                        result = dataOld?.FirstOrDefault();
                    }
                }
                
                if (result != null)
                {
                    var product_product = await _redisService.GetProductProductRedis(set);
                    var dataRaw = JsonConvert.DeserializeObject<PosStagingDto>(result.raw_data);
                   
                    if (dataRaw != null)
                    {
                        List<TransLineDto> transLines = new List<TransLineDto>();
                        if (dataRaw.Pos_Order_Line != null && dataRaw.Pos_Order_Line.Count > 0)
                        {
                            foreach (var item in dataRaw.Pos_Order_Line)
                            {
                                var itemData = product_product.Where(x => x.id == item.product_id).FirstOrDefault() ?? null;
                                var itemName = itemData.display_name;

                                if(itemData != null)
                                {
                                    var strCut = StringHelper.CutStringBuyChar(itemData.display_name, "[", "]");
                                    if (!string.IsNullOrEmpty(strCut))
                                    {
                                        itemName = itemName.Replace(strCut, "");
                                    }
                                }
                                int parentLineNo = item.id;
                                if (item.related_line_id > 0)
                                {
                                    parentLineNo = dataRaw.Pos_Order_Line.Where(x => x.fe_uid == item.related_line_id).Select(x=>x.id).FirstOrDefault();
                                }
                          
                                transLines.Add(new TransLineDto()
                                {
                                    LineNo = item.id,
                                    ParentLineNo = parentLineNo,
                                    ItemNo = item.product_id.ToString(),
                                    ItemName = itemName, 
                                    Barcode = "",
                                    Uom = item.uom_id.ToString(),
                                    UnitPrice = Math.Round(item.price_unit, 0),
                                    Qty = item.qty,
                                    DiscountAmount = Math.Round(item.price_unit * item.qty - item.price_subtotal_incl, 0),
                                    VatGroup = itemData != null ? itemData.tax_id.ToString() : "1",
                                    VatPercent = 10,
                                    VatAmount = Math.Round(item.price_subtotal_incl - item.price_subtotal, 0),
                                    LineAmountExcVAT = Math.Round(item.price_subtotal, 0),
                                    LineAmountIncVAT = Math.Round(item.price_subtotal_incl, 0),
                                    IsLoyalty = false,
                                    ItemType = (item.is_promotion_line == true && item.price_unit == 0) ? "reward_code".ToString().ToUpper() : "",
                                    Remark = GetRemark(conn, item, result.pos_reference),
                                    TransDiscountEntry = GetDiscountEntry(set, conn, item, result.pos_reference)
                                });
                            }
                            
                            List<TransPaymentEntryDto> transPaymentEntry = new List<TransPaymentEntryDto>();
                            if (dataRaw.Pos_Payment.Count > 0)
                            {
                                var payment_method_vcm = _redisService.GetPayment_VCM_Detail_Redis( null, false).Result ? .ToList();

                                foreach (var item in dataRaw.Pos_Payment)
                                {
                                    var paymentMethod = _redisService.GetPaymentMethodRedis(set, false).Result ? .Where(x=>x.id == item.payment_method_id).FirstOrDefault();
                                    if (!payment_method_vcm.Contains(item.payment_method_id))
                                    {
                                        transPaymentEntry.Add(new TransPaymentEntryDto()
                                        {
                                            LineNo = item.id,
                                            TenderType = paymentMethod != null ? paymentMethod.name.Replace(" ","") : "OTHER",
                                            PaymentAmount = item.amount,
                                            ReferenceNo = order_id,
                                            TransactionId = item.id.ToString(),
                                            PayForOrderNo = item.on_account_info,
                                            AdditionalData = null
                                        });
                                    }
                                }
                            }

                            var custInfo = GetMemberInfo(set, dataRaw.Pos_Order);
                            var employee = _redisService.GetEmployeeRedis(set, false).Result ? .Where(x=>x.id == dataRaw.Pos_Order.cashier_id).FirstOrDefault();

                            responseOrderDetail = new ResponseOrderDetail()
                            {
                                AppCode = "PLG",
                                OrderNo = dataRaw.Pos_Order.pos_reference,
                                OrderTime = dataRaw.Pos_Order.date_order.ToString("yyyy-MM-dd HH:mm:ss"),
                                CustNo = dataRaw.Pos_Order.partner_id.ToString(),
                                CustName = custInfo != null ? custInfo.Name.ToUpper() : "",
                                CustPhone = "",
                                CustAddress = "",
                                CustNote = dataRaw.Pos_Order.note_label,
                                DeliveryType = dataRaw.Pos_Order.sale_type_id,
                                CardMember = dataRaw.Pos_Order.partner_id.ToString(),
                                TotalAmount = Math.Round(dataRaw.Pos_Order.amount_total, 0),
                                PaymentAmount = transPaymentEntry != null ? transPaymentEntry.Sum(x => x.PaymentAmount) : 0,
                                Status = GetStatus(result.state, result.is_payment),
                                RefNo = dataRaw.Pos_Order.id.ToString(),
                                CashierId = employee != null ? employee.id.ToString() : "",
                                PromoAmount = dataRaw.Pos_Order.discount_amount * (-1),
                                PromoName = (dataRaw.Pos_Order.promotion_id > 0 && dataRaw.Pos_Order.discount_amount * (-1) > 0) ? (_redisService.GetPromoHeaderRedis(set, false).Result?.ToList().Where(x=>x.id == dataRaw.Pos_Order.promotion_id).FirstOrDefault().name) : "",
                                CashierName = employee != null ? employee.name.ToString() : "",
                                MemberInfo = custInfo,
                                StoreInfo = GetStoreInfo(set, dataRaw.Pos_Order),
                                TransLine = transLines,
                                TransPaymentEntry = transPaymentEntry
                            };

                        }
                    }
                }
                else
                {
                    responseOrderDetail = null;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("GetOrderAsync Exception " + ex.Message.ToString());
                responseOrderDetail = null;
                throw;
            }
            return responseOrderDetail;
        }
        public async Task<List<ResponseOrderList>> GetOrderListAsync(RequestOrderList request)
        {
            _logger.LogWarning(JsonConvert.SerializeObject(request));
            List<ResponseOrderList> responseOrderLists = new List<ResponseOrderList>();
            try
            {
                using IDbConnection conn = _context.CreateConnection(request.set);
                conn.Open();

                await _redisService.GetPayment_VCM_Redis(request.payment_method.ToList(), false);

                GetPosStagingAsync(conn, request, null);

                var result = await _redisService.GetListOrderRedis(_configuration["RedisConfig:RedisConnectionString"], _configuration["RedisConfig:Port"], request.location_id);

                //var result = conn.Query<Pos_Staging>(@"SELECT order_id, location_id, pos_reference, is_payment, is_display, state, raw_data, crt_date, upd_date FROM pos_staging WHERE is_display = true and location_id = " + request.location_id).ToList();
               
                if (result!= null && result.ToList().Count > 0)
                {
                    foreach (var item in result)
                    {
                        var dataRaw = JsonConvert.DeserializeObject<PosStagingDto>(item.raw_data);
                        if (dataRaw != null)
                        {
                            var pos_order_data = dataRaw.Pos_Order;
                            responseOrderLists.Add(new ResponseOrderList()
                            {
                                PartnerCode = "PLG",
                                StoreNo = request.location_id.ToString(),
                                OrderNo = pos_order_data.pos_reference.ToString(),
                                OrderDate = pos_order_data.date_order.ToString("yyyy-MM-dd HH:mm:ss"),
                                CustName = pos_order_data.note_label ?? "",
                                CustPhone = pos_order_data.mobile_receiver_info ?? "",
                                CustAddress = pos_order_data.note,
                                Status = GetStatus(item.state, item.is_payment),
                                CashierId = pos_order_data.cashier_id.ToString(),
                                CashierName = pos_order_data.cashier.ToString(),
                                TotalItem = ((int)dataRaw.Pos_Order_Line.Sum(x => x.qty)),
                                PaymentAmount = pos_order_data.amount_total,
                                //Remark = new Remark()
                                //{
                                //    Remark1 = OrderStatusConst.MappingOrderStatus()[GetStatus(item.state, item.is_payment)].ToString()
                                //}
                            });
                        }
                        else
                        {
                            responseOrderLists = null;
                        }
                    }
                }
                else
                {
                    responseOrderLists = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetOrderListAsync Exception " + ex.Message.ToString());
                throw;
            }
            return responseOrderLists;
        }
        public bool UpdateStatusOrderAsync(int set, string order_id, int status)
        {
            try
            {
                using IDbConnection conn = _context.CreateConnection(set);
                conn.Open();
                var pos_staging_data = conn.Query<Pos_Staging>(@"SELECT * FROM pos_staging WHERE pos_reference = '" + order_id + "';").FirstOrDefault();
                if (pos_staging_data == null)
                {
                    return false;
                }
                else
                {
                    using var transaction = conn.BeginTransaction();
                    if(status == 1 && pos_staging_data.is_payment == false)
                    {
                        var queryUpdPaid = @"UPDATE public.pos_staging SET is_display = false, is_payment = true, upd_date = now() WHERE is_payment = false AND pos_reference = '" + order_id + "';";
                        conn.Execute(queryUpdPaid);
                        _redisService.RemoveRedisKeyAsync(pos_staging_data.location_id + "." + order_id);
                    }
                    else if(status == 10 && pos_staging_data.state == "cancel")
                    {
                        var queryUpdCancel = @"UPDATE public.pos_staging SET is_display = false, is_payment = true, upd_date = now() WHERE state = 'cancel' AND is_display = true AND pos_reference = '" + order_id + "';";
                        conn.Execute(queryUpdCancel);
                        _redisService.RemoveRedisKeyAsync(pos_staging_data.location_id + "." + order_id);
                    }
                    else
                    {
                        return false;
                    }
                    
                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateStatusOrderAsync Exception " + ex.Message.ToString());
                throw;
            }
        }
        private int GetPosStagingAsync(IDbConnection conn, RequestOrderList request, string orderNo)
        {
            try
            {
                string currentDate = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");

                List<Pos_Order> lstPosOrder = null;
                List<Pos_Staging> lstPosOrderCancel = null;
                if (string.IsNullOrEmpty(orderNo))
                {
                    lstPosOrder = conn.Query<Pos_Order>(PosOrderQuery.get_pos_order_all(request.location_id, currentDate), new { payment_method_id = request.payment_method }).ToList();
                    lstPosOrderCancel = conn.Query<Pos_Staging>(PosOrderQuery.get_pos_order_cancel(currentDate, request.location_id)).ToList();
                }
                else
                {
                    lstPosOrder = conn.Query<Pos_Order>(PosOrderQuery.get_pos_order_by_order(orderNo)).ToList();
                }

                List<Pos_Staging> trans = new List<Pos_Staging>();
                if (lstPosOrder.Count > 0)
                {
                    var lstOrderId = new List<int>();
                    foreach (var o in lstPosOrder.ToList())
                    {
                        if(o.state == "paid")
                        {
                            lstOrderId.Add(o.id);
                        }
                    }

                    if (lstOrderId.Count > 0)
                    {
                        var lstPayment = conn.Query<Pos_Payment>(PosOrderQuery.get_pos_payment_by_order(), new { order_id = lstOrderId }).ToList();
                        var lstOrderLine = conn.Query<Pos_Order_Line>(PosOrderQuery.get_pos_order_line(), new { order_id = lstOrderId }).ToList();

                        foreach (var item in lstPosOrder.ToList())
                        {
                            trans.Add(new Pos_Staging()
                            {
                                order_id = item.id,
                                location_id = item.location_id,
                                is_display = true,
                                is_payment = false,
                                state = item.state,
                                pos_reference = item.pos_reference.Trim(),
                                raw_data = JsonConvert.SerializeObject(new PosStagingDto()
                                {
                                    Pos_Order = item,
                                    Pos_Order_Line = lstOrderLine.Where(x => x.order_id == item.id).ToList(),
                                    Pos_Payment = lstPayment.Where(x => x.pos_order_id == item.id).ToList()
                                }),
                                crt_date = DateTime.Now
                            });
                        }
                    }
                }
                using var transaction = conn.BeginTransaction();
                if (trans.Count > 0)
                {
                    var queryIns = @"INSERT INTO public.pos_staging (order_id, location_id, is_display, is_payment, state, pos_reference, raw_data, crt_date, upd_date) 
                                                                VALUES (@order_id, @location_id, @is_display, @is_payment, @state, @pos_reference, CAST(@raw_data AS json), now(), now());";
                    conn.Execute(queryIns, trans, transaction);
                    foreach (var item in trans)
                    {
                        _redisService.SetRedisKeyAsync(item.location_id + "." + item.pos_reference, JsonConvert.SerializeObject(item));
                    }
                }

                if (lstPosOrderCancel != null && lstPosOrderCancel.Count > 0)
                {
                    foreach (var item in lstPosOrderCancel)
                    {
                        _redisService.RemoveRedisKeyAsync(item.location_id + "." + item.pos_reference);

                        if(item.is_payment == true)
                        {
                            item.state = "cancel";

                            var queryUpd = @"UPDATE public.pos_staging SET is_display = true, state = 'cancel', upd_date = now() WHERE order_id = @order_id AND location_id = @location_id";
                            conn.Execute(queryUpd, item, transaction);

                            _redisService.SetRedisKeyAsync(item.location_id + "." + item.pos_reference, JsonConvert.SerializeObject(item));
                        }
                        else
                        {
                            var queryUpd = @"DELETE FROM public.pos_staging WHERE order_id = @order_id AND location_id = @location_id";
                            conn.Execute(queryUpd, item, transaction);
                        }
                    }
                }

                transaction.Commit();
                return lstPosOrder.Count();
            }
            catch(Exception ex)
            {
                _logger.LogError("GetPosStaging Exception " + ex.Message.ToString());
                return 0;
            }
        }
        private MemberInfoDto GetMemberInfo(int set, Pos_Order pos_Order)
        {

            var result = _redisService.GetMemberRedis(set,false).Result?.Where(x => x.id == pos_Order.partner_id).FirstOrDefault();
            if (result != null)
            {
                return new MemberInfoDto()
                {
                    MemberId = result.id.ToString(),
                    Name = result.name.ToUpper(),
                    Level = result.level_name,
                    CurrentPoint = Convert.ToInt32(pos_Order.partner_current_point),
                    TotalPoint = Convert.ToInt32(pos_Order.partner_total_point),
                    PointEarn = Convert.ToInt32(pos_Order.point_won),
                    ExpiredDate = pos_Order.partner_expired_date.ToString("yyyy-MM-dd")
                };
            }
            else
            {
                return null;
            }
        }
        private StoreInfoDto GetStoreInfo(int set, Pos_Order pos_Order)
        {
            var result = _redisService.GetPosConfigRedis(set, false).Result ? .Where(x=>x.store_id == pos_Order.location_id).FirstOrDefault();
            //Regex.Match(item.receipt_footer, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
            if (result != null)
            {
                var info = StringHelper.SliptString(result.receipt_header, "<br>");
                return new StoreInfoDto()
                {
                   StoreId = result.store_id,
                   StoreNo = result.store_no.ToString(),
                   StoreName = result.store_name,
                   PosNo = result.pos_no,
                   Address = (info != null && info.Length > 0) ? info[0].ToString() : "",
                   Phone = (info != null && info.Length > 1) ? info[1].ToString() : "",
                   OpenTime = (info != null && info.Length > 2) ? info[2].ToString() : "",
                   Email = "",
                   Hotline = "",
                   WebSite= "www.phuclong.com.vn",
                   ReceiptFooter = result.receipt_footer,
                   ReceiptHeader = result.receipt_header
                };
            }
            else
            {
                return null;
            }
        }
        private List<TransDiscountEntryDto> GetDiscountEntry(int set, IDbConnection conn, Pos_Order_Line pos_Order_Line, string order_name)
        {
            string query = string.Empty;
            List<TransDiscountEntryDto> lstDiscountEntry = new List<TransDiscountEntryDto>();
            var PromoHeader = _redisService.GetPromoHeaderRedis(set, false).Result ? .ToList();
            PromoHeaderOdooDto result = null;
            decimal discount = 0;

            if (pos_Order_Line.is_condition_line)
            {
                result = PromoHeader.Where(x => x.id == pos_Order_Line.promotion_condition_id).FirstOrDefault();
                discount = pos_Order_Line.discount_amount;
            }
            else if(pos_Order_Line.is_promotion_line)
            {
                result = PromoHeader.Where(x => x.id == pos_Order_Line.promotion_id).FirstOrDefault();
                discount = pos_Order_Line.discount_amount;
            }
            else if (pos_Order_Line.discount_amount > 0)
            {
                lstDiscountEntry.Add(new TransDiscountEntryDto()
                {
                    LineNo = pos_Order_Line.id + 3,
                    ParentLineNo = pos_Order_Line.id,
                    OfferNo = "",
                    OfferType = "DIS",
                    DiscountAmount = pos_Order_Line.discount_amount,
                    Qty = pos_Order_Line.qty,
                    Note = "DISCOUNT " + pos_Order_Line.discount_amount.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat)
                });
            }
            else if (pos_Order_Line.discount > 0)
            {
                lstDiscountEntry.Add(new TransDiscountEntryDto()
                {
                    LineNo = pos_Order_Line.id + 3,
                    ParentLineNo = pos_Order_Line.id,
                    OfferNo = "",
                    OfferType = "DIS",
                    DiscountAmount = Math.Round(pos_Order_Line.price_unit * pos_Order_Line.qty - pos_Order_Line.price_subtotal_incl, 0),
                    Qty = pos_Order_Line.qty,
                    Note = "DISCOUNT " + pos_Order_Line.discount.ToString("#,###", CultureInfo.GetCultureInfo("vi-VN").NumberFormat) + "%"
                });
            }

            if (pos_Order_Line.loyalty_discount_percent > 0)
            {
                lstDiscountEntry.Add(new TransDiscountEntryDto()
                {
                    LineNo = pos_Order_Line.id + 2,
                    ParentLineNo = pos_Order_Line.id,
                    OfferNo = "loyalty",
                    OfferType = "LOY",
                    DiscountAmount = 0,
                    Qty = pos_Order_Line.qty,
                    Note = "Giảm giá loyalty " + "10%"
                });
            }

            if (result != null && discount >0)
            {
                lstDiscountEntry.Add(new TransDiscountEntryDto()
                {
                    LineNo = pos_Order_Line.id + 1,
                    ParentLineNo = pos_Order_Line.id,
                    OfferNo = result.id.ToString(),
                    OfferType = result.list_type.ToString(),
                    DiscountAmount = discount,
                    Qty = pos_Order_Line.qty,
                    Note = result.name
                });

            }
            return lstDiscountEntry;
        }
        private Remark GetRemark(IDbConnection conn, Pos_Order_Line pos_Order_Line, string order_name)
        {
            try
            {
                if (pos_Order_Line.is_promotion_line = true && pos_Order_Line.price_unit == 0)
                {
                    string query_reward_code_info = @"select name as reward_code, publish_date, effective_date_from, effective_date_to, reward_code_publish_id from reward_code_info where pos_order_id ='" + order_name + @"';";
                    var reward_code_info = conn.Query<reward_code_info>(query_reward_code_info).FirstOrDefault();
                    string reward = reward_code_info != null ? reward_code_info.reward_code : "";

                    return new Remark()
                    {
                        Remark1 = "reward_code".ToString().ToUpper(),
                        Remark2 = reward,
                        Remark3 = reward_code_info != null ? reward_code_info.publish_date.ToString("yyyy-MM-dd") :"",
                        Remark4 = reward_code_info != null ? reward_code_info.effective_date_from.ToString("yyyy-MM-dd") :"",
                        Remark5 = reward_code_info != null ? reward_code_info.effective_date_to.ToString("yyyy-MM-dd") :"",
                        Remark6 = reward_code_info != null ? reward_code_info.reward_code_publish_id.ToString() : ""
                    };
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        private int GetStatus(string state, bool is_payment)
        {
            int status = 0;
            if(state == "cancel" && is_payment == false)
            {
                status = 11;
            }
            else if(state == "cancel" && is_payment == true)
            {
                status = 10;
            }
            else if (state == "paid" && is_payment == true)
            {
                status = 1;
            }          
            return status;
        }
        public async Task<Pos_Staging> GetOrderByIdAsync(int set,string order_id, int location_id)
        {
            try
            {
                Pos_Staging result =null;
                using IDbConnection conn = _context.CreateConnection(set);
                conn.Open();
                GetPosStagingAsync(conn, null, order_id);

                string jsData = await _redisService.GetRedisValueAsync(location_id + "." + order_id);

                var product_product = await _redisService.GetProductProductRedis(set);

                if (!string.IsNullOrEmpty(jsData))
                {
                    result = JsonConvert.DeserializeObject<Pos_Staging>(jsData);
                }

                return result;
            }
            catch
            {
                return null;
            }
        }
        public ResponseCheckOrder CheckOrderDetail(string order_no)
        {
            string query = @"select 'PLH' AppCode, a.name OrderNo, CAST(a.date_order as date) OrderDate, s.code StoreNo, s.name StoreName,
	                            t.sap_code SalesTypeId, t.name SalesTypeName, a.amount_total TotalAmount, 0 DiscountAmount, a.plh_partner_card_code MemberCardNumber,
	                            a.plh_earn LoyaltyPointsEarn, a.plh_redeem LoyaltyPointsRedeem, a.state Status, CAST(a.write_date::timestamp AT TIME ZONE 'UTC' as timestamp) CrtDate
                            from pos_order a
                            left join stock_warehouse s on a.warehouse_id = s.id
                            left join pos_sale_type t on t.id = a.sale_type_id
                            where a.state = 'paid'  and replace(a.name, '-', '') = '" + order_no + "';";
            try
            {
               
                using IDbConnection conn = _context.CreateConnection(1);
                conn.Open();
                var result =  conn.Query<check_order_detail>(query).FirstOrDefault();

                if(result != null)
                {
                    return new ResponseCheckOrder()
                    {
                        AppCode = result.appcode,
                        OrderNo = result.orderno,
                        OrderDate = result.orderdate,
                        StoreNo = result.storeno,
                        StoreName = result.storename,
                        SalesTypeId = result.salestypeid,
                        SalesTypeName = result.salestypename,
                        TotalAmount = result.totalamount,
                        DiscountAmount = result.discountamount,
                        CrtDate = result.crtdate,
                        MemberCardNumber = result.membercardnumber,
                        LoyaltyPointsEarn = result.loyaltypointsearn,
                        LoyaltyPointsRedeem = result.loyaltypointsredeem,
                        Status = result.status
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateStatusOrderAsync Exception " + ex.Message.ToString());
                throw;
            }
        }
    }
    public class reward_code_info
    {
        public string reward_code { get; set; }
        public DateTime  publish_date { get; set; }
        public DateTime effective_date_from { get; set; }
        public DateTime effective_date_to { get; set; }
        public int reward_code_publish_id { get; set; }
    }
}
