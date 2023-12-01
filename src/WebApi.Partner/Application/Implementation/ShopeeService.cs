using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Common.Helpers.Shopee;
using VCM.Common.Utils;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.API.Shopee;
using VCM.Shared.API.Shopee.Webhooks;
using VCM.Shared.Const;
using VCM.Shared.Dtos.PhucLong;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.Partner.Shopee;
using VCM.Shared.Entity.SalesPartner;
using VCM.Shared.Enums.Shopee;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Dapper;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;
using WebApi.Core.AppServices;

namespace VCM.Partner.API.Application.Implementation
{
    public class ShopeeService : IShopeeService
    {
        private readonly ILogger<ShopeeService> _logger;
        private readonly ITransService _transService;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly PartnerDbContext _dbConext;
        private readonly int[] lstStatus = new int[] { 3, 5, 6 };
        private readonly int[] lstStatusSuccess = new int[] { 3,2,1 };
        private int request_id = 0;
        private readonly string App_Code = "NOWFOOD";
        private readonly string PartnerMD = "PartnerMD";
        private readonly string[] itemCup = new string[] { "DUMMY101", "DUMMY102" };
        public ShopeeService(
          ILogger<ShopeeService> logger,
          ITransService transService,
          IMemoryCacheService memoryCacheService,
          PartnerDbContext dbConext
          )
        {
            _logger = logger;
            _transService = transService;
            _memoryCacheService = memoryCacheService;
            _dbConext = dbConext;
        }

        public async Task<ResponseClient> GetOrderDetail(RequestTransaction request, WebApiViewModel webApi, List<Item> itemDto, string proxy, string[] bypass)
        {
            ResponseClient resultObj = new ResponseClient();
            string mesage_error = string.Empty;
            int status_on_now = 0;
            try
            {
                string storeNo = request.PosNo[..4];
                var restaurant = _memoryCacheService.GetShopeeRestaurantAsync().Result?.Where(x => x.partner_restaurant_id == request.PosNo.Substring(0, 4)).FirstOrDefault();
                if (restaurant == null)
                {
                    return ResponseHelper.RspNotFoundData(storeNo + @" chưa được khai báo trên NowFood");
                }
                else
                {
                    var checkData = _dbConext.Shopee_update_order.Where(x => x.order_code == request.OrderNo && x.partner_restaurant_id == storeNo && x.status >= 3).OrderByDescending(x => x.Id).FirstOrDefault();
                    if(checkData != null)
                    {
                        if (checkData.status == 8 && checkData.update_flg == "N")
                        {
                            return ResponseHelper.RspNotFoundData("Đơn hàng " + request.OrderNo + @" đã bị hủy");
                        }
                        else if(checkData.status != 8 && checkData.update_flg == "Y")
                        {
                            return ResponseHelper.RspNotFoundData("Đơn hàng " + request.OrderNo + @" đã được chế biến");
                        }
                    }
                    else
                    {
                        return ResponseHelper.RspNotFoundData(request.OrderNo + @" không tìm thấy đơn hàng");
                    }

                    string function = "OrderDetail";
                    var router = webApi.WebRoute.Where(x => x.Name == function).FirstOrDefault();

                    OrderDetailShopeeRequest jsonBody = new OrderDetailShopeeRequest()
                    {
                        order_code = request.OrderNo
                    };

                    Dictionary<string, string> wsRequest = new Dictionary<string, string>
                    {
                        { "order_code", jsonBody.order_code }
                    };

                    string url = webApi.Host + router.Route;
                    string jsonString = JsonConvert.SerializeObject(jsonBody);
                    string sign = ShopeeUtils.CreateSignature(webApi.Password, ShopeeUtils.CreateBaseString("POST", url, jsonString));
                   
                    var result = ShopeeApiHelper.PostApi(url, router.Version, webApi.UserName, sign, null, jsonString, proxy, bypass, ref request_id);
                    _logger.LogWarning("===> Shopee request_id: " + request_id.ToString() + " ===> response: " + result);
                    
                    if (!string.IsNullOrEmpty(result))
                    {


                        var rspObj = JsonConvert.DeserializeObject<OrderDetailShopeeRsp>(result);

                        if (rspObj.result == ResultCodeShopee.success.ToString())
                        {
                            var itemMaster = _memoryCacheService.GetItemPhucLongAsync().Result;
                            var resultMapping = MappingOrderBody(JsonConvert.SerializeObject(rspObj.reply), storeNo, itemMaster, checkData.status, ref mesage_error, ref status_on_now);
                            if(resultMapping == null)
                            {
                                resultObj.Meta = ResponseHelper.MetaOK(201, mesage_error);
                            }
                            else
                            {
                                if (checkData.status == 8 && checkData.update_flg == "Y" && status_on_now == 8)
                                {
                                    resultMapping.Status = 8;
                                }
                                else if (checkData.status == 8 && checkData.update_flg == "N")
                                {
                                    resultObj.Meta = ResponseHelper.MetaOK(201, mesage_error);
                                }

                                resultObj.Meta = ResponseHelper.MetaOK(200, "OK");
                                resultObj.Data = resultMapping;
   
                            }
                            //update
                            checkData.serial = rspObj.reply.serial;
                            checkData.raw_data = JsonConvert.SerializeObject(rspObj.reply).ToString();
                            checkData.message += !string.IsNullOrEmpty(mesage_error) ? mesage_error: "OK";
                            await Shopee_update_serial_order(checkData);
                        }
                        else
                        {
                            resultObj.Meta = ResponseHelper.MetaOK(201, rspObj.result.ToString());
                        }
                    }
                    else
                    {
                        resultObj = ResponseHelper.RspNotFoundData(result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetOrderDetail Exception:" + ex.Message.ToString());
                resultObj = ResponseHelper.RspTryCatch(ex);
            }
            await Task.Delay(1);
            return resultObj;
        }
        public async Task<ResponseClient> GetOrderList(RequestListOrderPOS request, WebApiViewModel webApi, string proxy, string[] bypass)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string StoreNo = request.PosNo.Substring(0, 4);
                var restaurant = _memoryCacheService.GetShopeeRestaurantAsync().Result?.Where(x => x.partner_restaurant_id == StoreNo).FirstOrDefault();
                if (restaurant == null)
                {
                    _logger.LogWarning(StoreNo + @" chưa được khai báo trên NowFood");
                    return ResponseHelper.RspNotExistsStoreNo(StoreNo + @" chưa được khai báo trên NowFood");
                }
                else
                {
                    string query = @"SELECT A.*
                                    FROM Shopee_update_order (NOLOCK) A
                                    INNER JOIN (
		                                    SELECT order_code, partner_restaurant_id, MAX(Id) AS Id
		                                    FROM Shopee_update_order (NOLOCK)
		                                    WHERE update_flg = 'N' AND update_type = 2 AND partner_restaurant_id = '" + StoreNo + @"'
		                                    GROUP BY order_code, partner_restaurant_id
	                                    ) B ON A.Id = B.Id
                                    WHERE A.pick_time IS NOT NULL AND A.partner_restaurant_id = '" + StoreNo  + @"' and A.update_flg = 'N' AND A.[status] IN (3,5) AND CAST(A.crt_date as DATE) >= CAST(getdate() AS DATE)
                                    ORDER BY A.crt_date;";

                    var lstOrderNow = _dbConext.Shopee_update_order.FromSqlRaw(query).ToList();
                    var lstOrder = new List<ResponseOrderList>();
                    if (lstOrderNow.Count > 0)
                    {
                        foreach (var item in lstOrderNow)
                        {
                            int status = 0;
                            if (!lstStatus.Contains(item.status))
                            {
                                status = 1;
                            }
                            lstOrder.Add(new ResponseOrderList()
                            {
                                PartnerCode = request.PartnerCode,
                                AppCode = request.AppCode,
                                StoreNo = StoreNo,
                                OrderDate = item.pick_time ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                OrderNo = item.order_code,
                                CustName = "",
                                CustAddress = "pick_time: " + item.pick_time??DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                CustPhone = "",
                                TotalItem = 1,
                                PaymentAmount = 0,
                                CashierId = "NOW",
                                CashierName = "NOW",
                                Status = status,
                                Remark = null
                            }) ;
                        }
                    }
                    resultObj = ResponseHelper.RspOK(lstOrder);
                }
            }
            catch(Exception ex)
            {
                resultObj = ResponseHelper.RspTryCatch(ex);
            }
            await Task.Delay(1);
            return resultObj;
        }
        public async Task<ResponseClient> UpdateStatusOrder(RequestUpdateOrderStatus request, WebApiViewModel webApi, string proxy, string[] bypass)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                var restaurant = _memoryCacheService.GetShopeeRestaurantAsync().Result?.Where(x => x.partner_restaurant_id == request.PosNo.Substring(0, 4)).FirstOrDefault();
                if (restaurant == null)
                {
                    return ResponseHelper.RspNotFoundData(request.PosNo.Substring(0, 4) + @" chưa được khai báo trên NowFood");
                }
                else
                {
                    //=================================== CHECK order status
                    var updateData = _dbConext.Shopee_update_order.Where(x => x.order_code == request.OrderNo).OrderByDescending(x => x.Id).ToList();
                    var lastUpdateData = updateData.OrderByDescending(x => x.Id).FirstOrDefault();
                    var checkUpdate = updateData.Where(x => x.status == 5).FirstOrDefault();
                    
                    if(updateData.Where(x =>x.update_flg == "Y").FirstOrDefault() != null)
                    {
                        return ResponseHelper.RspNotWarning(201, @"Đơn hàng " + request.OrderNo + " đã được chế biến");
                    }

                    if (lastUpdateData == null)
                    {
                        return ResponseHelper.RspNotWarning(201, @"Đơn hàng " + request.OrderNo + " không tìm thấy");
                    }
                    else if (checkUpdate != null && lstStatus.Contains(checkUpdate.status) && checkUpdate.update_flg == "Y")
                    {
                        return ResponseHelper.RspNotWarning(201, @"Đơn hàng " + request.OrderNo + " đã thực hiện thời gian: " + checkUpdate.chg_date.ToString("dd-MM-yyyy HH:mm:ss"));
                    }
                    else if (lastUpdateData != null && lstStatusSuccess.Contains(lastUpdateData.status) && lastUpdateData.update_flg == "N")
                    {
                        await Shopee_update_status_order(null, updateData);
                        _logger.LogWarning("===> Shopee auto comfirm: " + JsonConvert.SerializeObject(updateData));
                        return new ResponseClient()
                        {
                            Meta = ResponseHelper.MetaOK(200, "Successfully"),
                            Data = null
                        };
                    }
                    else if (lastUpdateData != null && lastUpdateData.status == 3 && lastUpdateData.update_flg == "Y")
                    {
                        return ResponseHelper.RspNotWarning(201, @"Đơn hàng " + request.OrderNo + " đã được thanh toán");
                    }
                    else if (lastUpdateData != null && lastUpdateData.status == 8)
                    {
                        return ResponseHelper.RspNotWarning(201, @"Đơn hàng " + request.OrderNo + " đã hủy trên ứng dụng NowFood");
                    }

                    //=================================== Call Shopee update status
                    string function = "UpdateStatusOrder";
                    var router = webApi.WebRoute.Where(x => x.Name == function).FirstOrDefault();
                    _logger.LogWarning("===> Shopee request: " + function + " - route: " + webApi.Host + router.Route);
                    OrderUpdateShopee jsonBody = new OrderUpdateShopee()
                    {
                        order_code = request.OrderNo,
                        serial = lastUpdateData.serial,
                        status = (int)OrderUpdateStatusShopee.CONFIRM,
                        restaurant_id = restaurant.restaurant_id
                    };

                    string payload = JsonConvert.SerializeObject(jsonBody).ToString();
                    string baseString = ShopeeUtils.CreateBaseString("POST", webApi.Host + router.Route, payload);

                    string sign = ShopeeUtils.CreateSignature(webApi.Password, baseString);

                    _logger.LogWarning("===> Shopee request: " + payload);
                    var result = ShopeeApiHelper.PostApi(webApi.Host + router.Route, router.Version, webApi.UserName, sign, null, payload, proxy, bypass, ref request_id);
                    _logger.LogWarning("===> Shopee request_id: " + request_id.ToString() + " ===>" + result);
                   
                    if (string.IsNullOrEmpty(result))
                    {
                        return ResponseHelper.RspNotFoundData(@"Lỗi cập nhật đơn hàng " + request.OrderNo + " ");
                    }

                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    string message = resultRsp.result.ToString();

                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        updateData.ForEach(x => { if (x.status == 3) x.crt_user = "API"; });
                        await Shopee_update_status_order(resultRsp, updateData);
                        message = resultRsp.result.ToString();
                        
                        resultObj = new ResponseClient()
                        {
                            Meta = ResponseHelper.MetaOK(200, message),
                            Data = null
                        };
                    }
                    else
                    {
                        message = result;
                        var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                        string[] now_status_update = new string[] { "error_forbidden", "error_out_of_update_order", "error_access_token", "error_version_too_low", "error_server"};
                        if (resultShopee != null && now_status_update.Contains(resultShopee.result))
                        {
                            updateData.ForEach(x => { if (x.status == 5 || x.status == 3) x.crt_user = request.PosNo; });
                            updateData.ForEach(x => { if (x.status == 5 || x.status == 3) x.message = resultShopee.result; });
                            await Shopee_update_status_order(resultRsp, updateData);
                            resultObj = new ResponseClient()
                            {
                                Meta = ResponseHelper.MetaOK(200, message),
                                Data = null
                            };
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspNotWarning(201, message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> Shopee.UpdateStatusOrder: " + ex.Message.ToString());
                resultObj = ResponseHelper.RspTryCatch(ex);
            }
          
            return resultObj;
        }
        private OrderResponseBody MappingOrderBody(string jsonData, string storeNo,List<ItemDto> itemMaster, int order_status, ref string message_error, ref int status_on_now)
        {
            List<RspOrderLineDto> items = new List<RspOrderLineDto>();
            var data = JsonConvert.DeserializeObject<OrderDetailShopee>(jsonData);
            int status = 1;
            var list_dish_nowfood = GetShopeeDishesCache().Where(x=>x.StoreNo == storeNo).ToList();
            if (lstStatus.Contains(order_status))
            {
                status = 0;
            }
            else 
            {
                status = 8;
                message_error = @"Đơn hàng " + data.code + " đã bị hủy - " + data.cs_note ?? "";
            }
            status_on_now = data.status;

            //header
            decimal total_bill_on_now = data.commission_amount.value + data.total_value.value;
            var result = new OrderResponseBody
            {
                AppCode = App_Code,
                Status = status,
                OrderNo = data.code,
                OrderDate = data.order_time,
                CustPhone = (data.customer!= null) ? data.customer.phone :"",
                CustAddress = (data.restaurant != null) ? data.restaurant.name :"",
                Note = GetNoteForOrder(data),
                CustName = data.status.ToString() + ".NowFood - " + ((data.customer != null) ? data.customer.name : ""),
                PaymentAmount = 0,
                TotalAmount = 0,
                SaleTypeId = 30,
                StoreNo = storeNo,
                ShippingFee = (data.customer_bill != null) ? data.customer_bill.shipping_fee.value : 0,
                HasVatInvoice = false,
                BillingInfo = null,
                UseCoupon = null,
                Serial = data.serial??"",
                WinCode = ""
            };
            if (data.dish_groups.Count > 0)
            {
                foreach (var dgroup in data.dish_groups)
                {
                    foreach(var item in dgroup.dishes)
                    {
                        decimal origin_price_value = item.original_price.value;
                        decimal price_value = item.price.value;
                        decimal discount_amount_value = Math.Round((origin_price_value - price_value) * item.quantity, 0);
                        decimal origin_price_topping_value = 0;

                        List<RspOrderLineDto> lineTopping = new List<RspOrderLineDto>();
                        var lstLineOption = new List<RspOrderLineOptionDto>();
                        string cup = string.Empty;
                        if ((!string.IsNullOrEmpty(item.partner_dish_id) && item.partner_dish_id[..8] == "PHUCLONG") || item.dish_name.ToUpper().Contains("COMBO"))
                        {
                            List<RspOrderLineDto> items_topping = new List<RspOrderLineDto>();
                            if (item.topping_groups.Count > 0)
                            {
                                foreach (var gtopping in item.topping_groups)
                                {
                                    int checkLineTopping = gtopping.toppings.Count;
                                    int loopTopping = 0;
                                    foreach (var topping in gtopping.toppings)
                                    {
                                        loopTopping++;
                                        var itemNo_topping = "";
                                        if (!string.IsNullOrEmpty(topping.partner_topping_id))
                                        {
                                            itemNo_topping = topping.partner_topping_id.Substring(0, 8);
                                        }
                                        else
                                        {
                                            message_error = @"Topping khuyến mãi " + item.partner_dish_id + " chưa được mapping với ItemNo Phúc Long";
                                            return null;
                                        }

                                        var itemMapping = itemMaster.Where(x => x.ItemNo == itemNo_topping).FirstOrDefault();
                                        var dishOnNowFood = list_dish_nowfood.Where(x => x.ItemNo == itemNo_topping).FirstOrDefault();
                                        if (itemMapping == null || dishOnNowFood == null)
                                        {
                                            message_error = @"Sản phẩm " + topping.partner_topping_id + " chưa được mapping với ItemNo Phúc Long";
                                            return null;
                                        }

                                        var lineItem = new RspOrderLineDto
                                        {
                                            LineId = topping.topping_id,
                                            ParentLineId = item.dish_id,
                                            ItemNo = itemNo_topping,
                                            ItemName = itemMapping.ItemName,
                                            Uom = itemMapping.Uom,
                                            OldPrice = dishOnNowFood.UnitPrice,
                                            UnitPrice = dishOnNowFood.UnitPrice,
                                            Quantity = item.quantity,
                                            DiscountAmount = 0,
                                            LineAmount = dishOnNowFood.UnitPrice * item.quantity,
                                            TaxGroupCode = itemMapping.VatGroup,
                                            VatPercent = itemMapping.VatPercent,
                                            Note = item.note ?? "",
                                            IsLoyalty = false,
                                            CupType = dishOnNowFood.CupType,
                                            Size = itemMapping.ItemType ?? "",
                                            IsTopping = false,
                                            IsCombo = false,
                                            ComboNo = item.partner_dish_id.ToUpper(),
                                            ArticleType = dgroup.dish_group_id.ToString(),
                                            Barcode = item.dish_id.ToString(),
                                            OptionEntry = null,
                                            DiscountType = gtopping.partner_topping_group_id.ToUpper(),
                                        };
                                        items_topping.Add(lineItem);
                                    }
                                }
                            }

                            if (items_topping.Count > 0)
                            {
                                //GetComboNo(gtopping.partner_topping_group_id.ToUpper())
                                foreach (var top in items_topping)
                                {
                                    if (top.ComboNo.Contains("GIFT"))
                                    {
                                        top.DiscountAmount = top.LineAmount;
                                        top.LineAmount = 0;
                                        if(top.DiscountAmount > 0)
                                        {
                                            var discount = new OrderDiscount()
                                            {
                                                LineId = top.LineId + 1,
                                                OfferNo = GetOfferNo(top.ComboNo),
                                                OfferType = "NOW",
                                                Quantity = top.Quantity,
                                                DiscountAmount = top.DiscountAmount,
                                                Note = top.ComboNo
                                            };
                                            top.DiscountEntry = new List<OrderDiscount> { discount }.ToList();
                                        }
                                    }
                                }

                                decimal total_amount_in_items_topping = items_topping.Sum(x => x.LineAmount);
                                decimal total_distcount_amount_in_promo = (total_amount_in_items_topping - item.total.value) > 0 ? (total_amount_in_items_topping - item.total.value) : 0;
                                if (total_distcount_amount_in_promo > 0)
                                {
                                    DistributionDiscountToLine(items_topping, total_distcount_amount_in_promo, item.partner_dish_id, item.dish_name);
                                }
                                foreach (var t in items_topping)
                                {
                                    items.Add(t);
                                }
                                //_logger.LogWarning("items_topping result:" + JsonConvert.SerializeObject(items_topping));
                            }
                        }
                        else
                        {
                            if (item.topping_groups.Count > 0)
                            {
                                foreach (var gtopping in item.topping_groups)
                                {
                                    foreach (var topping in gtopping.toppings)
                                    {
                                        if (!gtopping.partner_topping_group_id.ToUpper().Contains("CHON_SIZE"))
                                        {
                                            //Option
                                            if (topping.price.value == 0 && !itemCup.Contains(topping.partner_topping_id))
                                            {
                                                //Topping gift
                                                if (gtopping.partner_topping_group_id.ToUpper().Contains("GIFT"))
                                                {
                                                    _logger.LogWarning(gtopping.partner_topping_group_id.ToUpper() + "_" + topping.partner_topping_id);
                                                    if (!string.IsNullOrEmpty(topping.partner_topping_id))
                                                    {
                                                        topping.partner_topping_id = StringHelper.Left(topping.partner_topping_id, 8);
                                                    }
                                                    var itemMappingTopping = itemMaster.Where(x => x.ItemNo == topping.partner_topping_id).FirstOrDefault();
                                                    if (itemMappingTopping == null)
                                                    {
                                                        message_error = @"Topping được tặng " + topping.topping_name + " chưa được mapping với ItemNo SAP - Phúc Long";
                                                        _logger.LogWarning(data.code + ":" + message_error);
                                                        return null;
                                                    }
                                                    var toppingGift = new RspOrderLineDto()
                                                    {
                                                        LineId = item.order_dish_id,
                                                        ParentLineId = item.order_dish_id,
                                                        ItemNo = topping.partner_topping_id,
                                                        ItemName = itemMappingTopping.ItemName??"",
                                                        Uom = itemMappingTopping.Uom,
                                                        OldPrice = topping.original_price.value,
                                                        UnitPrice = topping.original_price.value,
                                                        Quantity = item.quantity,
                                                        DiscountAmount = topping.original_price.value - topping.price.value,
                                                        LineAmount = topping.price.value * item.quantity,
                                                        TaxGroupCode = itemMappingTopping.VatGroup,
                                                        VatPercent = itemMappingTopping.VatPercent,
                                                        Note = item.note ?? "",
                                                        IsLoyalty = false,
                                                        CupType = itemMappingTopping.CupType ?? "",
                                                        Size = itemMappingTopping.ItemType ?? "",
                                                        IsTopping = true,
                                                        IsCombo = false,
                                                        ComboNo = "",
                                                        ArticleType = "",
                                                        Barcode = topping.topping_id.ToString()
                                                    };
                                                    lineTopping.Add(toppingGift);
                                                }
                                                else
                                                {
                                                    lstLineOption.Add(new RspOrderLineOptionDto()
                                                    {
                                                        LineId = topping.topping_id,
                                                        Type = "Option",
                                                        ItemNo = topping.topping_id.ToString(),
                                                        Uom = "DV",
                                                        OptionType = StringHelper.GetFirstSplit(topping.partner_topping_id, '_'),
                                                        OptionName = topping.topping_name,
                                                        Description = "",
                                                        Note = gtopping.group_name,
                                                        Qty = topping.quantity,
                                                        ItemNoRef = topping.partner_topping_id ?? "",
                                                    });
                                                }
                                            }
                                            //Topping sales
                                            else if (topping.price.value > 0 && gtopping.partner_topping_group_id != item.partner_dish_id)
                                            {
                                                origin_price_topping_value += topping.price.value;
                                                if (!string.IsNullOrEmpty(topping.partner_topping_id))
                                                {
                                                    topping.partner_topping_id = StringHelper.Left(topping.partner_topping_id, 8);
                                                }
                                                var itemMappingTopping = itemMaster.Where(x => x.ItemNo == topping.partner_topping_id).FirstOrDefault();
                                                if (itemMappingTopping == null)
                                                {
                                                    message_error = @"Sản phẩm " + topping.topping_name + " chưa được mapping với ItemNo SAP - Phúc Long";
                                                    _logger.LogWarning(data.code + ":" + message_error);
                                                    return null;
                                                }
                                                var toppingSales = new RspOrderLineDto()
                                                {
                                                    LineId = item.order_dish_id,
                                                    ParentLineId = item.order_dish_id,
                                                    ItemNo = topping.partner_topping_id,
                                                    ItemName = itemMappingTopping.ItemName + " " + itemMappingTopping.ItemType ?? "",
                                                    Uom = itemMappingTopping.Uom,
                                                    OldPrice = topping.original_price.value,
                                                    UnitPrice = topping.original_price.value,
                                                    Quantity = item.quantity,
                                                    DiscountAmount = topping.original_price.value - topping.price.value,
                                                    LineAmount = topping.price.value * item.quantity,
                                                    TaxGroupCode = itemMappingTopping.VatGroup,
                                                    VatPercent = itemMappingTopping.VatPercent,
                                                    Note = item.note ?? "",
                                                    IsLoyalty = false,
                                                    CupType = itemMappingTopping.CupType??"",
                                                    Size = itemMappingTopping.ItemType ?? "",
                                                    IsTopping = false,
                                                    IsCombo = false,
                                                    ComboNo = "",
                                                    ArticleType = "Topping".ToUpper(),
                                                    Barcode = topping.topping_id.ToString(),
                                                    DiscountType = gtopping.partner_topping_group_id.ToUpper(),
                                                };
                                                if (topping.original_price.value - topping.price.value > 0)
                                                {
                                                    var discountEntry = new List<OrderDiscount>
                                                    {
                                                        new OrderDiscount()
                                                        {
                                                            LineId = topping.topping_id,
                                                            OfferNo = GetOfferNo(topping.partner_topping_id),
                                                            OfferType = "NOW",
                                                            Quantity = item.quantity,
                                                            DiscountAmount = topping.original_price.value - topping.price.value,
                                                            Note = "Topping khuyến mãi gạch giá"
                                                        }
                                                    };
                                                    toppingSales.DiscountEntry = discountEntry;
                                                }
                                                lineTopping.Add(toppingSales);
                                            }
                                        }
                                        else if (gtopping.partner_topping_group_id.ToUpper().Contains("CHON_SIZE"))
                                        {
                                            item.partner_dish_id = StringHelper.Left(item.partner_dish_id, 8);
                                            if (!string.IsNullOrEmpty(item.partner_dish_id) && topping.price.value > 0)
                                            {
                                                var getParentCode = list_dish_nowfood.Where(x => x.ItemNo == item.partner_dish_id).FirstOrDefault();
                                                if(getParentCode!= null)
                                                {
                                                    var bigSize = list_dish_nowfood.Where(x => x.ParentCode == getParentCode.ParentCode && x.ItemNo != item.partner_dish_id).FirstOrDefault();
                                                    if(bigSize!= null)
                                                    {
                                                        item.partner_dish_id = bigSize.ItemNo;
                                                    }
                                                    else
                                                    {
                                                        message_error = @"Sản phẩm " + item.partner_dish_id + "-" + item.dish_name + " chưa được mapping " + gtopping.toppings.FirstOrDefault().topping_name??"";
                                                        _logger.LogWarning(data.code + ":" + message_error);
                                                        return null;
                                                    }
                                                }
                                                else
                                                {
                                                    message_error = @"Sản phẩm " + item.partner_dish_id + "-" + item.dish_name + " chưa được mapping " + gtopping.toppings.FirstOrDefault().topping_name ?? "";
                                                    _logger.LogWarning(data.code + ":" + message_error);
                                                    return null;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(item.partner_dish_id))
                            {
                                item.partner_dish_id = StringHelper.Left(item.partner_dish_id, 8);
                            }

                            var itemMapping = itemMaster.Where(x => x.ItemNo == item.partner_dish_id).FirstOrDefault();
                            if (itemMapping == null)
                            {
                                message_error = @"Sản phẩm " + item.partner_dish_id + "-" + item.dish_name + " chưa được mapping với ItemNo Phúc Long";
                                _logger.LogWarning(data.code + ":" + message_error);
                                return null;
                            }
                            origin_price_value -= origin_price_topping_value;

                            //Update size for CUP
                            lstLineOption.ForEach(x => x.Note = itemMapping.ItemType ?? "");

                            discount_amount_value = discount_amount_value > 0 ? discount_amount_value : 0;

                             var lineItem = new RspOrderLineDto
                             {
                                LineId = item.order_dish_id,
                                ParentLineId = item.order_dish_id,
                                ItemNo = item.partner_dish_id,
                                ItemName = itemMapping.ItemName,
                                Uom = itemMapping.Uom,
                                OldPrice = origin_price_value,
                                UnitPrice = origin_price_value,
                                Quantity = item.quantity,
                                DiscountAmount = discount_amount_value > 0 ? discount_amount_value : 0,
                                LineAmount = origin_price_value * item.quantity - discount_amount_value,
                                TaxGroupCode = itemMapping.VatGroup,
                                VatPercent = itemMapping.VatPercent,
                                Note = item.note ?? "",
                                IsLoyalty = false,
                                CupType = itemMapping.CupType??"",
                                Size = itemMapping.ItemType ?? "",
                                IsTopping = false,
                                IsCombo = false,
                                ComboNo = item.partner_dish_id,
                                ArticleType = dgroup.dish_group_id.ToString(),
                                Barcode = item.dish_id.ToString(),
                                OptionEntry = lstLineOption,
                                DiscountType = item.partner_dish_id,
                             };

                            //discountEntry
                            if (discount_amount_value > 0)
                            {
                                var discountEntry = new List<OrderDiscount>
                            {
                                new OrderDiscount()
                                {
                                    LineId = item.dish_id,
                                    OfferNo = GetOfferNo(item.partner_dish_id),
                                    OfferType = "NOW",
                                    Quantity = item.quantity,
                                    DiscountAmount = discount_amount_value,
                                    Note = "Khuyến mãi gạch giá"
                                }
                            };
                                lineItem.DiscountEntry = discountEntry;
                            }
                            items.Add(lineItem);
                        }

                        if (lineTopping!= null && lineTopping.Count > 0)
                        {
                            lineTopping.ForEach(x=>items.Add(x));
                        }
                    }
                }
                AddCupTypeForItem(items, list_dish_nowfood);
                result.Items = items.ToList();
            }

            //check discount_commission
            decimal discount_total_bill = 0;
            decimal total_bill_amount = items.Sum(x => x.LineAmount);
            decimal pos_commission = Math.Round((total_bill_amount * 6) / 100, 0);
            if(pos_commission != data.commission_amount.value)
            {
                decimal discount_amount_on_now = Math.Round((data.commission_amount.value * 100) / 6, 0);
                discount_total_bill = total_bill_amount - (discount_amount_on_now > 0 ? discount_amount_on_now : 0);
                if (discount_total_bill > 0)
                {
                    total_bill_amount -= discount_total_bill;
                    DistributionDiscountToLine(items, discount_total_bill);
                }
            }

            _logger.LogWarning("items result:" + JsonConvert.SerializeObject(items));
            //update LineId
            UpdateSttForLineItem(items);
            CheckTotalAmountPOSvsNOW(items, total_bill_on_now);
            total_bill_amount = items.Sum(x => x.LineAmount);

            if(items.Where(x=>x.DiscountAmount < 0).FirstOrDefault() != null)
            {
                message_error = @"Đơn hàng " + result.OrderNo + " có chênh lệch giảm giá";
                return null;
            }

            //paymentEntry
            var payments = new List<OrderPaymentDto>
                {
                    new OrderPaymentDto()
                    {
                        LineId = 1,
                        PaymentMethod = "DV20",
                        AmountTendered = total_bill_amount,
                        AmountInCurrency = total_bill_amount,
                        CurrencyCode = "VND",
                        ExchangeRate = 1,
                        TransactionNo = data.code??"",
                        ApprovalCode =data.code??"",
                        TraceCode = ""
                    },
                };
            result.Payments = payments;
            result.TotalAmount = total_bill_amount;
            result.PaymentAmount = total_bill_amount;
            return result;
        }
        public async Task<bool> Shopee_export_error_menu(export_error_menu request)
        {
            try
            {
                _dbConext.Shopee_export_error_menu.Add(new Shopee_export_error_menu()
                {
                    object_id = request.object_id,
                    object_type = request.object_type,
                    restaurant_id = request.restaurant_id,
                    partner_restaurant_id = request.partner_restaurant_id,
                    partner_object_id = request.partner_object_id,
                    event_id = request.event_id,
                    update_type = request.update_type,
                    error_type = request.error_type,
                    error_time = request.error_time,

                });
                await _dbConext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> Shopee_export_error_menu.Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> Shopee_update_drivers_status(partner_api_url_callback_update_drivers_status request)
        {
            try
            {
                if (request.driver_arriving_times.Count > 0)
                {
                    foreach (var item in request.driver_arriving_times)
                    {
                        var driver_arriving_time = new Shopee_update_drivers_status()
                        {
                            order_code = item.order_code,
                            restaurant_id = item.restaurant_id,
                            partner_restaurant_id = item.partner_restaurant_id,
                            driver_uid = item.driver_uid,
                            arriving_time = item.arriving_time
                        };
                        _dbConext.Shopee_update_drivers_status.Add(driver_arriving_time);
                    }
                    await _dbConext.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> Shopee_update_menu.Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> Shopee_update_menu(Partner_api_url_callback request)
        {
            try
            {
                if (request.object_changes.Count > 0)
                {
                    foreach (var item in request.object_changes)
                    {
                        _dbConext.Shopee_update_menu.Add(new Shopee_update_menu()
                        {
                            event_id = item.event_id,
                            object_id = item.object_id,
                            object_type = item.object_type,
                            update_type = item.update_type,
                            restaurant_id = item.restaurant_id,
                            partner_object_id = item.partner_object_id,
                            partner_restaurant_id = item.partner_restaurant_id
                        });
                    }
                    await _dbConext.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> Shopee_update_menu.Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> Shopee_update_order(Partner_api_url_callback_update_order request)
        {
            try
            {
                if (request.update_type == 1)
                {
                    return true;
                }

                string storeNo = "";
                var shopee_restaurant = _memoryCacheService.GetShopeeRestaurantAsync().Result;
                if (shopee_restaurant != null)
                {
                    storeNo = shopee_restaurant.Where(x => x.restaurant_id == request.restaurant_id).FirstOrDefault().partner_restaurant_id ?? "";
                }

                _dbConext.Shopee_update_order.Add(new Shopee_update_order()
                {
                    order_code = request.order_code,
                    update_type = request.update_type,
                    restaurant_id = request.restaurant_id,
                    pick_time = request.pick_time??"",
                    status = request.status,
                    merchant_note = request.merchant_note ?? "",
                    note_for_shipper = request.note_for_shipper ?? "",
                    partner_restaurant_id = storeNo,
                    serial = request.serial,
                    update_flg = "N",
                    message = "",
                    chg_date = DateTime.Now,
                    crt_user = "NOW"
                });
                await _dbConext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> Shopee_update_order.Exception: " + ex.Message.ToString());
                return false;
            }
        }
        private string GetApiListOrder(RequestListOrderPOS request, WebApiViewModel webApi, string proxy, string[] bypass, ShopeeRestaurant restaurant)
        {
            try
            {
                string function = "ListOrder";
                var router = webApi.WebRoute.Where(x => x.Name == function).FirstOrDefault();
                _logger.LogWarning("===> Shopee request: " + function + " - route: " + webApi.Host + router.Route);

                string restaurant_id = request.PosNo.Substring(0, 4).ToString();
                string from_date = DateTime.Now.ToString("yyyy-MM-dd");
                OrderListShopeeRequest jsonBody = new OrderListShopeeRequest()
                {
                    restaurant_id = restaurant.restaurant_id,
                    partner_restaurant_id = restaurant.partner_restaurant_id,
                    from_date = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"),
                    to_date = DateTime.Now.ToString("yyyy-MM-dd"),
                    limit = request.PageSize,
                    offset = 0,
                    status = (int)OrderListStatusShopee.CONFIRMED
                };

                string payload = JsonConvert.SerializeObject(jsonBody).ToString();
                string baseString = ShopeeUtils.CreateBaseString("POST", webApi.Host + router.Route, payload);

                string sign = ShopeeUtils.CreateSignature(webApi.Password, baseString);

                _logger.LogWarning("===> Shopee request: " + payload);
                var result = ShopeeApiHelper.PostApi(webApi.Host + router.Route, router.Version, webApi.UserName, sign, null, payload, proxy, bypass, ref request_id);
                _logger.LogWarning("===> Shopee request_id: " + request_id.ToString() + " ===>" + result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> Shopee GetApiListOrder Exception: " + ex.Message.ToString());
                return null;
            }
        }
        private async Task<bool> Shopee_update_status_order(ResultShopeeRsp result, List<Shopee_update_order> updateData)
        {
            try
            {
                if(result != null)
                {
                    updateData.ForEach(x => x.message = x.message + "|API confirm success");
                }
                else
                {
                    updateData.ForEach(x => x.message = "Nowfood auto confirm success");
                }

                updateData.ForEach(x => x.update_flg = "Y");
                updateData.ForEach(x => _dbConext.Update(x));
                await _dbConext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> Shopee_update_status_order Exception: " + JsonConvert.SerializeObject(result));
                _logger.LogWarning("===> Shopee_update_status_order Exception: " + ex.Message.ToString());
                return false;
            }
        }
        private async Task<bool> Shopee_update_serial_order(Shopee_update_order updateData)
        {
            try
            {
                updateData.chg_date = DateTime.Now;
                _dbConext.Update(updateData);
                await _dbConext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> Shopee_update_serial_order Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public async Task<ResponseClient> CountedOrder(CountOrderRequest request, WebApiViewModel webApi)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                var restaurant = _memoryCacheService.GetShopeeRestaurantAsync().Result?.Where(x => x.partner_restaurant_id == request.StoreNo).FirstOrDefault();
                if (restaurant == null)
                {
                    _logger.LogWarning(request.StoreNo + @" chưa được khai báo trên NowFood");
                    return ResponseHelper.RspNotExistsStoreNo(request.StoreNo + @" chưa được khai báo trên NowFood");
                }
                else
                {
                    string query = @"SELECT A.*
                                    FROM Shopee_update_order (NOLOCK) A
                                    INNER JOIN (
		                                    SELECT order_code, partner_restaurant_id, MAX(Id) AS Id
		                                    FROM Shopee_update_order (NOLOCK)
		                                    WHERE update_flg = 'N' AND update_type = 2 AND partner_restaurant_id = '" + request.StoreNo + @"'
		                                    GROUP BY order_code, partner_restaurant_id
	                                    ) B ON A.Id = B.Id
                                    WHERE A.pick_time IS NOT NULL AND A.partner_restaurant_id = '" + request.StoreNo + @"' and A.update_flg = 'N' AND A.[status] IN (3,5) AND CAST(A.crt_date as DATE) >= CAST(getdate() AS DATE)
                                    ORDER BY A.crt_date;";

                    var lstOrderNow = _dbConext.Shopee_update_order.FromSqlRaw(query).ToList();
                    

                    resultObj = new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = "OK"
                        },
                        Data = new CountOrderResponse()
                        {
                            PartnerCode = request.PartnerCode,
                            AppCode = request.AppCode,
                            StoreNo = request.StoreNo,
                            Counted = lstOrderNow.Count > 0 ? lstOrderNow.Count : 0
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> ShopeeService.CountedOrder Exception:" + ex.Message.ToString());
                resultObj = ResponseHelper.RspTryCatch(ex);
            }
            await Task.Delay(1);
            return resultObj;
        }
        private void DistributionDiscountToLine(List<RspOrderLineDto> items, decimal discount_amount, string OfferNo = null, string lable_discount = null)
        {
            int maxLine = items.Where(x => x.IsTopping == false && x.LineAmount > 0).Count();
            decimal total_amount_distribution = items.Where(x => x.IsTopping == false && x.LineAmount > 0).Sum(x => x.LineAmount);
            decimal temp_discount = 0;
            int i = 0;

            foreach (var item in items.OrderByDescending(x=>x.LineAmount))
            {
                i++;
                if (item.DiscountType.Contains("MAIN"))
                {
                    continue;
                }

                var percent_on_line_amount = Math.Round((item.LineAmount * 100) / total_amount_distribution, 0);
                var dis_discount_amount = Math.Round((discount_amount * percent_on_line_amount) / 100, 0);
                
                if(i == maxLine)
                {
                    if (dis_discount_amount != discount_amount)
                    {
                        dis_discount_amount -= (temp_discount - discount_amount);
                    }
                }

                if (dis_discount_amount > 0)
                {
                    if(dis_discount_amount > item.LineAmount)
                    {
                        dis_discount_amount = item.LineAmount;
                    }

                    item.LineAmount -= dis_discount_amount;
                    item.DiscountAmount += dis_discount_amount;

                    if (item.DiscountEntry != null && item.DiscountEntry.Count > 0)
                    {
                        item.DiscountEntry.FirstOrDefault().DiscountAmount += dis_discount_amount;
                    }
                    else
                    {
                        var discount = new OrderDiscount()
                        {
                            LineId = i * 100,
                            OfferNo = GetOfferNo(item.ComboNo),
                            OfferType = "NOW",
                            Quantity = item.Quantity,
                            DiscountAmount = dis_discount_amount,
                            Note = lable_discount ?? "Giảm giá"
                        };
                        item.DiscountEntry = new List<OrderDiscount>{discount}.ToList();
                    }
                }
                temp_discount += dis_discount_amount;
            }
        }
        private void UpdateSttForLineItem(List<RspOrderLineDto> items)
        {
            int add_stt = 0;
            foreach (var item in items)
            {
                add_stt++;
                item.LineId = add_stt;
                if (item.IsTopping)
                {
                    var item_pa = items.Where(x => x.ParentLineId == item.ParentLineId).FirstOrDefault();
                    int id_new = item_pa.LineId;
                    item.ParentLineId = items.Where(x => x.ParentLineId == item.ParentLineId).FirstOrDefault().LineId;
                }
                

                if (item.DiscountEntry != null && item.DiscountEntry.Count > 0)
                {
                    int stt_discount = 0;
                    foreach (var d in item.DiscountEntry)
                    {
                        stt_discount++;
                        d.LineId = stt_discount;
                    }

                }
                if (item.OptionEntry != null && item.OptionEntry.Count > 0)
                {
                    int stt_op = 0;
                    foreach (var d in item.OptionEntry.ToList())
                    {
                        stt_op++;
                        d.LineId = stt_op;
                        if (d.OptionName.ToUpper().Contains("ĐÁ CHUNG") || (d.OptionName.ToUpper().Contains("ĐÁ") && d.OptionName.ToUpper().Contains("CHUNG")))
                        {
                            item.OptionEntry.Remove(d);
                        }
                    }

                }
            }
            foreach (var item in items)
            {
                if (!item.IsTopping)
                {
                    item.ParentLineId = 0;
                }
            }

        }
        private void AddCupTypeForItem(List<RspOrderLineDto> items, List<ItemSalesOnApp> itemSalesOnApps)
        {
            foreach(var item in items)
            {
                var checkCupType = itemSalesOnApps.Where(x => x.ItemNo == item.ItemNo && itemCup.Contains(x.CupType)).FirstOrDefault();
                if (checkCupType != null)
                {
                    var cup = new RspOrderLineOptionDto()
                    {
                        LineId = item.LineId * 10,
                        Type = "Cup",
                        ItemNo = checkCupType.CupType,
                        Uom = "CAI",
                        OptionType = "",
                        OptionName = "",
                        Description = (checkCupType.CupType == "DUMMY101") ? "Ly nhựa" : "Ly giấy",
                        Note = item.Size,
                        Qty = item.Quantity,
                        ItemNoRef = checkCupType.CupType
                    };
                    if(item.OptionEntry != null && item.OptionEntry.Count > 0)
                    {
                        item.OptionEntry.Add(cup);
                    }
                    else
                    {
                        List<RspOrderLineOptionDto> options = new List<RspOrderLineOptionDto>();
                        options.Add(cup);
                        item.OptionEntry = options;
                    }
                    
                }
            }
        }
        private void CheckTotalAmountPOSvsNOW(List<RspOrderLineDto> items, decimal total_amount_on_now)
        {
            decimal total_amount_on_pos = items.Where(x => x.LineAmount > 0).Sum(x => x.LineAmount);
            decimal chenh_lech = total_amount_on_now - total_amount_on_pos;
            _logger.LogWarning("@chenh_lech: " + chenh_lech.ToString());
            
            if (chenh_lech < 0)
            {
                foreach (var item in items.Where(x => x.LineAmount > 0).OrderByDescending(x => x.LineAmount))
                {
                    if (item.DiscountEntry != null && item.DiscountEntry.Count > 0)
                    {
                        decimal update_chenh_lech = 0;
                        var check_diff_am = item.DiscountAmount + chenh_lech;
                        if (item.DiscountAmount - chenh_lech < 0)
                        {
                            update_chenh_lech = item.DiscountAmount;
                            chenh_lech += item.DiscountAmount;
                        }
                        else
                        {
                            update_chenh_lech = chenh_lech;
                            chenh_lech = 0;
                        }

                        var updateDiscountEntry = item.DiscountEntry.Where(x => x.DiscountAmount > 0).FirstOrDefault();
                        if (updateDiscountEntry != null)
                        {
                            item.DiscountEntry.Where(x => x.LineId == updateDiscountEntry.LineId).FirstOrDefault().DiscountAmount = updateDiscountEntry.DiscountAmount + update_chenh_lech;
                            item.LineAmount -= update_chenh_lech;
                            item.DiscountAmount += update_chenh_lech;

                            _logger.LogWarning(item.LineId.ToString() + " @LineAmount: " + item.LineAmount.ToString());
                        }

                        update_chenh_lech = 0;
                        _logger.LogWarning("@chenh_lech: " + chenh_lech.ToString());
                        if (chenh_lech == 0)
                        {
                            break;
                        }
                    }
                }
            }
            if (chenh_lech > 0)
            {
                foreach (var item in items.Where(x=>x.LineAmount > 0).OrderByDescending(x=>x.LineAmount))
                {
                    if(item.DiscountEntry != null && item.DiscountEntry.Count > 0)
                    {
                        decimal update_chenh_lech = 0;
                        var check_diff_duong = item.DiscountAmount - chenh_lech;
                        if(item.DiscountAmount - chenh_lech < 0)
                        {
                            update_chenh_lech = item.DiscountAmount;
                            chenh_lech -= item.DiscountAmount;
                        }
                        else
                        {
                            update_chenh_lech = chenh_lech;
                            chenh_lech = 0;
                        }

                        var updateDiscountEntry = item.DiscountEntry.Where(x => x.DiscountAmount > 0).FirstOrDefault();
                        if (updateDiscountEntry != null)
                        {
                            item.DiscountEntry.Where(x => x.LineId == updateDiscountEntry.LineId).FirstOrDefault().DiscountAmount = updateDiscountEntry.DiscountAmount - update_chenh_lech;
                            item.LineAmount += update_chenh_lech;
                            item.DiscountAmount -= update_chenh_lech;

                            _logger.LogWarning(item.LineId.ToString() + " @LineAmount: " + item.LineAmount.ToString());
                        }

                        update_chenh_lech = 0;
                        _logger.LogWarning("@chenh_lech: " + chenh_lech.ToString());
                        if (chenh_lech == 0)
                        {
                            break;
                        }
                    }
                }
            }

            if (chenh_lech == 0)
            {
                return;
            }
        }
        public List<ItemSalesOnApp> GetShopeeDishesCache(bool isDelete = false)
        {
            try
            {
                if (isDelete)
                {
                     _memoryCacheService.RemoveRedisValueAsync(RedisConst.Redis_ShopeeDish);
                }
                var key_redis =  _memoryCacheService.GetRedisValueAsync(RedisConst.Redis_ShopeeDish).Result;
                if (!string.IsNullOrEmpty(key_redis))
                {
                    return JsonConvert.DeserializeObject<List<ItemSalesOnApp>>(key_redis);
                }
                else
                {
                    var sysConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == "PLH" && x.Name.ToUpper() == PartnerMD.ToUpper()).FirstOrDefault();
                    if (sysConfig != null)
                    {
                        DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                        using var conn = dapperDbContext.CreateConnDB;
                        conn.Open();
                        string procedure = @"SELECT * FROM dbo.ItemSalesOnApp (NOLOCK);";
                        var data =  conn.Query<ItemSalesOnApp>(procedure).ToList();

                        if (data.Count > 0)
                        {
                            _memoryCacheService.SetRedisKeyAsync(RedisConst.Redis_ShopeeDish, JsonConvert.SerializeObject(data));
                        }
                        return data.ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> GetShopeeDishesCache Exception: " + ex.Message.ToString());
                return null;
            }
        }
        private string GetNoteForOrder(OrderDetailShopee data)
        {
            string note = "";
            decimal total_bill_on_now = data.commission_amount.value + data.total_value.value;
            note = "SPF:" + StringHelper.Right(data.code,5) + "|" + total_bill_on_now.ToString("#,##0");
            if (!string.IsNullOrEmpty(data.note))
            {
                note += "|" + data.note;
            }
            if (!string.IsNullOrEmpty(data.cs_note))
            {
                note += "|" + data.cs_note;
            }
            return note;
        }
        private string GetComboNo(string str)
        {
            string result = "";
            if (!string.IsNullOrEmpty(str))
            {
                if (str.Contains("GIFT"))
                {
                    result = "GIFT";
                }
                if (str.Contains("MAIN"))
                {
                    result = "MAIN";
                }
            }
            return result;
        }
        private string GetOfferNo(string str)
        {
            try
            {
                var arr = StringHelper.SliptString(str, "_");
                if(arr.Count() > 0)
                {
                    return arr[1];
                }
                return str;
            }
            catch
            {
                return str;
            }
        }
    }
}
