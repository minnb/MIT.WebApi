using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VCM.Common.Helpers.Shopee;
using VCM.Common.Utils;
using VCM.Shared.API.Shopee;
using VCM.Shared.API.Shopee.Restaurants;
using VCM.Shared.Const;
using VCM.Shared.Enums.Shopee;

namespace WebApi.Core.AppServices.ShopeeService
{
    public interface IRestaurantShopeeService
    {
        List<RestaurantHeader> GetRestaurantInfo(string url, string app_id, string app_key, string partner_restaurant_id, string proxy, string[] bypass, ref string message);
        List<Store_operation_time_ranges> Get_Store_operation_time_ranges(string url, string app_id, string app_key, string partner_restaurant_id, string proxy, string[] bypass, ref string message);
        List<Store_operation_time_ranges> Set_Store_operation_time_ranges(string url, string app_id, string app_key, string partner_restaurant_id, string proxy, string[] bypass, ref string message);
    }
    public class RestaurantShopeeService : IRestaurantShopeeService
    {
        private readonly ILogger<RestaurantShopeeService> _logger;
        private int request_id = 0;
        public RestaurantShopeeService
            (
                ILogger<RestaurantShopeeService> logger
            )
        {
            _logger = logger;
        }
        public List<RestaurantHeader> GetRestaurantInfo(string url, string app_id, string app_key, string partner_restaurant_id, string proxy, string[] bypass, ref string message)
        {
            try
            {
                var data = new RestaurantInfoRequest()
                {
                    partner_restaurant_id = partner_restaurant_id
                };

                string jsonBody = JsonConvert.SerializeObject(data);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, ShopeeConst.ShopeeVersion, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                _logger.LogWarning(result);

                if (!string.IsNullOrEmpty(result))
                {
                    var result_topping_groups = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = result_topping_groups.result.ToString();
                    if (result_topping_groups.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(result_topping_groups.reply);
                        return JsonConvert.DeserializeObject<RestaurantInfoResponse>(jsonStr).restaurants;
                    }
                    else
                    {
                        message = result;
                        return null;
                    }
                }
                else
                {
                    message = result;
                    return null;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> GetRestaurantInfo.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public List<Store_operation_time_ranges> Get_Store_operation_time_ranges(string url, string app_id, string app_key, string partner_restaurant_id, string proxy, string[] bypass, ref string message)
        {
            try
            {
                var data = new RestaurantInfoRequest()
                {
                    partner_restaurant_id = partner_restaurant_id
                };

                string jsonBody = JsonConvert.SerializeObject(data);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, ShopeeConst.ShopeeVersion, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                _logger.LogWarning(result);

                if (!string.IsNullOrEmpty(result))
                {
                    var result_topping_groups = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = result_topping_groups.result.ToString();
                    if (result_topping_groups.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(result_topping_groups.reply);
                        return JsonConvert.DeserializeObject<List<Store_operation_time_ranges>>(jsonStr);
                    }
                    else
                    {
                        message = result;
                        return null;
                    }
                }
                else
                {
                    message = result;
                    return null;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> Get_Store_operation_time_ranges.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public List<Store_operation_time_ranges> Set_Store_operation_time_ranges(string url, string app_id, string app_key, string partner_restaurant_id, string proxy, string[] bypass, ref string message)
        {
            try
            {
                var data = new RestaurantInfoRequest()
                {
                    partner_restaurant_id = partner_restaurant_id
                };

                string jsonBody = JsonConvert.SerializeObject(data);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, ShopeeConst.ShopeeVersion, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                _logger.LogWarning(result);

                if (!string.IsNullOrEmpty(result))
                {
                    var result_reply = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = result_reply.result.ToString();
                    if (result_reply.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(result_reply.reply);
                        return JsonConvert.DeserializeObject<List<Store_operation_time_ranges>>(jsonStr);
                    }
                    else
                    {
                        message = result;
                        return null;
                    }
                }
                else
                {
                    message = result;
                    return null;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> Set_Store_operation_time_ranges.Exception: " + ex.Message.ToString());
                return null;
            }
        }
    }
}
