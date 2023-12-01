using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VCM.Common.Helpers.Shopee;
using VCM.Common.Utils;
using VCM.Shared.API.Shopee;
using VCM.Shared.API.Shopee.Topping;
using VCM.Shared.Const;
using VCM.Shared.Enums.Shopee;

namespace WebApi.Core.AppServices.ShopeeService
{
    public interface IToppingShopeeService
    {
        List<get_topping_groups> GetToppingGroupShopee(string url, string app_id, string app_key, int restaurant_id, string proxy, string[] bypass, ref string message);
        ToppingCreateResponse CreateToppingShopee(string url, string app_id, string app_key, string proxy, string[] bypass, ToppingCreateRequest request, ref string message);
        ToppingCreateGroupResponse ToppingCreateGroupShopee(string url, string app_id, string app_key, string proxy, string[] bypass, ToppingCreateGroupRequest request, ref string message);
        List<toppings> GetToppingShopee(string url, string app_id, string app_key, int restaurant_id, string proxy, string[] bypass, ref string message);
        bool MappingToppingShopee(string url, string app_id, string app_key, ToppingMappingRequest request, string proxy, string[] bypass, ref string message);
        bool MappingToppingGroupShopee(string url, string app_id, string app_key, ToppingGroupMappingRequest request, string proxy, string[] bypass, ref string message);
        bool Topping_update_group_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_update_group request, ref string message);
        bool Topping_delete_group_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_delete_group request, ref string message);
        ToppingCreateResponse Topping_update_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_update request, ref string message);
        bool Topping_delete_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_delete request, ref string message);
        bool Topping_update_prices_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_update_prices request, ref string message);
        bool Topping_set_group_quantity(string url, string app_id, string app_key, string proxy, string[] bypass, topping_set_group_quantity request, ref string message);
        bool topping_set_statuses_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_set_statuses request, ref string message);
    }

    public class ToppingShopeeService: IToppingShopeeService
    {
        private readonly ILogger<ToppingShopeeService> _logger;
        private readonly string _versionApi = ShopeeConst.ShopeeVersion;
        private int request_id = 0;
        public ToppingShopeeService
            (
                ILogger<ToppingShopeeService> logger
            )
        {
            _logger = logger;
        }

        public ToppingCreateResponse CreateToppingShopee(string url, string app_id, string app_key, string proxy, string[] bypass, ToppingCreateRequest request, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, ShopeeConst.ShopeeVersion, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return JsonConvert.DeserializeObject<ToppingCreateResponse>(JsonConvert.SerializeObject(resultRsp.reply));
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
                _logger.LogWarning("===> IToppingShopeeService.CreateToppingShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public List<get_topping_groups> GetToppingGroupShopee(string url, string app_id, string app_key, int restaurant_id, string proxy, string[] bypass,ref string message)
        {
            try
            {
                url = url + "?restaurant_id=" + restaurant_id;
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("GET", url, null));
                var result = ShopeeApiHelper.GetApi(url, _versionApi, app_id, sign, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var result_topping_groups = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = result_topping_groups.result.ToString();
                    if(result_topping_groups.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(result_topping_groups.reply);
                        _logger.LogWarning(jsonStr);

                        return JsonConvert.DeserializeObject<ToppingGroupResponse>(jsonStr).topping_groups; 
                    }
                    else
                    {
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
                _logger.LogWarning("===> GetToppingGroupShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public List<toppings> GetToppingShopee(string url, string app_id, string app_key, int restaurant_id, string proxy, string[] bypass ,ref string message)
        {
            try
            {
                url = url + "?restaurant_id=" + restaurant_id;
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("GET", url, null));
                var result = ShopeeApiHelper.GetApi(url, _versionApi, app_id, sign, proxy, bypass, ref request_id);

                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(resultRsp.reply);
                        return JsonConvert.DeserializeObject<ToppingGetResponse>(jsonStr).toppings;
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
                _logger.LogWarning("===> GetToppingShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public bool MappingToppingGroupShopee(string url, string app_id, string app_key, ToppingGroupMappingRequest request, string proxy, string[] bypass, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, _versionApi, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return true;
                    }
                    else
                    {
                        message = result;
                        return false;
                    }
                }
                else
                {
                    message = result;
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> MappingToppingGroupShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public bool MappingToppingShopee(string url, string app_id, string app_key, ToppingMappingRequest request, string proxy, string[] bypass, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, _versionApi, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return true;
                    }
                    else
                    {
                        message = result;
                        return false;
                    }
                }
                else
                {
                    message = result;
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> MappingToppingShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public ToppingCreateGroupResponse ToppingCreateGroupShopee(string url, string app_id, string app_key, string proxy, string[] bypass, ToppingCreateGroupRequest request, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, ShopeeConst.ShopeeVersion, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return JsonConvert.DeserializeObject<ToppingCreateGroupResponse>(JsonConvert.SerializeObject(resultRsp.reply));
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
                _logger.LogWarning("===> IToppingShopeeService.ToppingCreateGroupShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public bool Topping_delete_group_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_delete_group request, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, _versionApi, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return true;
                    }
                    else
                    {
                        message = result;
                        return false;
                    }
                }
                else
                {
                    message = result;
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> Topping_delete_group_shopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool Topping_delete_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_delete request, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, _versionApi, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return true;
                    }
                    else
                    {
                        message = result;
                        return false;
                    }
                }
                else
                {
                    message = result;
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> Topping_delete_shopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool Topping_set_group_quantity(string url, string app_id, string app_key, string proxy, string[] bypass, topping_set_group_quantity request, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, ShopeeConst.ShopeeVersion, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return true;
                    }
                    else
                    {
                        message = result;
                        return false;
                    }
                }
                else
                {
                    message = result;
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> IToppingShopeeService.Topping_set_group_quantity.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool topping_set_statuses_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_set_statuses request, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, _versionApi, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return true;
                    }
                    else
                    {
                        message = result;
                        return false;
                    }
                }
                else
                {
                    message = result;
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> topping_set_statuses_shopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool Topping_update_group_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_update_group request, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, _versionApi, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return true;
                    }
                    else
                    {
                        message = result;
                        return false;
                    }
                }
                else
                {
                    message = result;
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> Topping_update_group.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool Topping_update_prices_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_update_prices request, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, ShopeeConst.ShopeeVersion, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return true;
                    }
                    else
                    {
                        message = result;
                        return false;
                    }
                }
                else
                {
                    message = result;
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> IToppingShopeeService.Topping_update_prices.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public ToppingCreateResponse Topping_update_shopee(string url, string app_id, string app_key, string proxy, string[] bypass, topping_update request, ref string message)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("POST", url, jsonBody));
                var result = ShopeeApiHelper.PostApi(url, ShopeeConst.ShopeeVersion, app_id, sign, null, jsonBody, proxy, bypass, ref request_id);
                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        message = resultRsp.result.ToString();
                        return JsonConvert.DeserializeObject<ToppingCreateResponse>(JsonConvert.SerializeObject(resultRsp.reply));
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
                _logger.LogWarning("===> IToppingShopeeService.Topping_update_shopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }
    }
}
