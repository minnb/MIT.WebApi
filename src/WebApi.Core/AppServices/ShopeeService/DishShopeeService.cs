using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VCM.Common.Helpers.Shopee;
using VCM.Common.Utils;
using VCM.Shared.API.Shopee;
using VCM.Shared.API.Shopee.Dish;
using VCM.Shared.API.Shopee.Restaurants;
using VCM.Shared.Const;
using VCM.Shared.Enums.Shopee;

namespace WebApi.Core.AppServices.ShopeeService
{
    public interface IDishShopeeService
    {
        List<DishesShopee> GetDishShopee(string url, string app_id, string app_key, int restaurant_id, string proxy, string[] bypass, ref string message);
        bool MappingDishShopee(string url, string app_id, string app_key, DishMappingsRequest request, string proxy, string[] bypass, ref string message);
        DishCreateResponse CreateDishShopee(string url, string app_id, string app_key, string proxy, string[] bypass, object request, ref string message);
        DishPictureRsp dish_upload_picture(string url, string app_id, string app_key, string proxy, string[] bypass, dish_upload_picture request, ref string message);
        DishCreateResponse CreateBulkDishShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishBulkCreateRequest request, ref string message);
        bool UpdateDishShopee(string url, string app_id, string app_key, string proxy, string[] bypass, object request, ref string message);
        bool UpdateDishBulkShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishBulkCreateRequest request, ref string message);
        bool UpdatePartneGroupIdMappingShopee(string url, string app_id, string app_key, string proxy, string[] bypass, Dish_update_partner_group_id_mappings request, ref string message);
        bool DeleteDishShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishDeleteRequest request, ref string message);
        bool DeleteDishToppingMappingShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishDeleteToppingMappingRequest request, ref string message);
        List<DishGroupShopee> GetDishGroupShopee(string url, string app_id, string app_key, int restaurant_id, string proxy, string[] bypass, ref string message);
        List<DishGroupsInfo> Get_dish_partner_group_id_mappings(string url, string app_id, string app_key, int restaurant_id, string proxy, string[] bypass, ref string message);
        DishGroupCreateResponse CreateDishGroupShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishGroupCreateRequest request, ref string message);
        Dish_get_group_detail Dish_get_group_detail_Shopee(string url, string app_id, string app_key, string proxy, string[] bypass, int restaurant_id, string partner_restaurant_id, int dish_group_id, string partner_dish_group_id, ref string message);
        Dish_get_detail Dish_get_detail_Shopee(string url, string app_id, string app_key, string proxy, string[] bypass,int restaurant_id, string partner_restaurant_id, int dish_id, ref string message);
        bool UpdateDishGroupShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishGroupCreateRequest request, ref string message);
        bool DeleteDishGroupShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishGroupDeleteRequest request, ref string message);
        bool Dish_delete_picture(string url, string app_id, string app_key, string proxy, string[] bypass, dish_delete_picture request, ref string message);
        bool DishCreateToppingMappingShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishCreateToppingMappingRequest request, ref string message);
        bool DishSetStatusShopee(string url, string app_id, string app_key, string proxy, string[] bypass, object request, ref string message);
        List<DishGetOutOfServiceResult> GetOutOfServiceShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishGetOutOfServiceRequest request, ref string message);
        Get_approval_status_rsp Get_approval_status(string url, string app_id, string app_key, string proxy, string[] bypass, Get_approval_status request, ref string message);
    }
    public class DishShopeeService : IDishShopeeService
    {
        private readonly ILogger<DishShopeeService> _logger;
        private int request_id = 0;
        public DishShopeeService
            (
                ILogger<DishShopeeService> logger
            )
        {
            _logger = logger;
        }

        public DishCreateResponse CreateBulkDishShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishBulkCreateRequest request, ref string message)
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
                        return JsonConvert.DeserializeObject<DishCreateResponse>(JsonConvert.SerializeObject(resultRsp.reply));
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
                _logger.LogWarning("===> IDishShopeeService.CreateBulkDishShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public DishGroupCreateResponse CreateDishGroupShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishGroupCreateRequest request, ref string message)
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
                        return JsonConvert.DeserializeObject<DishGroupCreateResponse>(JsonConvert.SerializeObject(resultRsp.reply));
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
                _logger.LogWarning("===> IDishShopeeService.CreateDishShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public DishCreateResponse CreateDishShopee(string url, string app_id, string app_key, string proxy, string[] bypass, object request, ref string message)
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
                        return JsonConvert.DeserializeObject<DishCreateResponse>(JsonConvert.SerializeObject(resultRsp.reply));
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
                _logger.LogWarning("===> IDishShopeeService.CreateDishShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public bool DeleteDishGroupShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishGroupDeleteRequest request, ref string message)
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
                _logger.LogWarning("===> DeleteDishGroupShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool DeleteDishShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishDeleteRequest request, ref string message)
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
                        message = resultRsp.result.ToString() + "@" + JsonConvert.SerializeObject(resultRsp.reply);
                        return true;
                    }
                    else
                    {
                        message = result;
                        return true;
                    }
                }
                else
                {
                    message = result;
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> IDishShopeeService.DeleteDishShopee.Exception: " + ex.Message.ToString());
                return true;
            }
        }

        public bool DeleteDishToppingMappingShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishDeleteToppingMappingRequest request, ref string message)
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
                        message = resultRsp.result.ToString() + "@" + JsonConvert.SerializeObject(resultRsp.reply);
                        return true;
                    }
                    else
                    {
                        message = result;
                        return true;
                    }
                }
                else
                {
                    message = result;
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message.ToString();
                _logger.LogWarning("===> IDishShopeeService.DeleteDishShopee.Exception: " + ex.Message.ToString());
                return true;
            }
        }

        public bool DishCreateToppingMappingShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishCreateToppingMappingRequest request, ref string message)
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
                _logger.LogWarning("===> GetToppingGroupShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool DishSetStatusShopee(string url, string app_id, string app_key, string proxy, string[] bypass, object request, ref string message)
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
                _logger.LogWarning("===> DishSetStatusShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool Dish_delete_picture(string url, string app_id, string app_key, string proxy, string[] bypass, dish_delete_picture request, ref string message)
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
                _logger.LogWarning("===> Dish_delete_picture.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public Dish_get_detail Dish_get_detail_Shopee(string url, string app_id, string app_key, string proxy, string[] bypass,int restaurant_id, string partner_restaurant_id, int dish_id, ref string message)
        {
            try
            {
                url = url + "?restaurant_id=" + restaurant_id + "&partner_restaurant_id=" + partner_restaurant_id + "&dish_id=" + dish_id;
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("GET", url, null));
                var result = ShopeeApiHelper.GetApi(url, ShopeeConst.ShopeeVersion, app_id, sign, proxy, bypass, ref request_id);

                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    _logger.LogWarning(JsonConvert.SerializeObject(resultRsp));
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(resultRsp.reply);

                        return JsonConvert.DeserializeObject<Dish_get_detail>(jsonStr);
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
                _logger.LogWarning("===> GetDishGroupShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public Dish_get_group_detail Dish_get_group_detail_Shopee(string url, string app_id, string app_key, string proxy, string[] bypass,int restaurant_id, string partner_restaurant_id, int dish_group_id, string partner_dish_group_id, ref string message)
        {
            try
            {
                url = url + "?restaurant_id=" + restaurant_id + "&partner_restaurant_id=" + partner_restaurant_id + "&dish_group_id=" + dish_group_id + "&partner_dish_group_id=" + partner_dish_group_id;
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("GET", url, null));
                var result = ShopeeApiHelper.GetApi(url, ShopeeConst.ShopeeVersion, app_id, sign, proxy, bypass, ref request_id);

                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    _logger.LogWarning(JsonConvert.SerializeObject(resultRsp));
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(resultRsp.reply);

                        return JsonConvert.DeserializeObject<Dish_get_group_detail>(jsonStr);
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
                _logger.LogWarning("===> Dish_get_group_detail.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public DishPictureRsp dish_upload_picture(string url, string app_id, string app_key, string proxy, string[] bypass, dish_upload_picture request, ref string message)
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
                        return JsonConvert.DeserializeObject<DishPictureRsp>(JsonConvert.SerializeObject(resultRsp.reply));
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
                _logger.LogWarning("===> IDishShopeeService.dish_upload_picture.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public List<DishGroupShopee> GetDishGroupShopee(string url, string app_id, string app_key, int restaurant_id, string proxy, string[] bypass, ref string message)
        {
            try
            {
                url = url + "?restaurant_id=" + restaurant_id;
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("GET", url, null));
                var result = ShopeeApiHelper.GetApi(url, ShopeeConst.ShopeeVersion, app_id, sign, proxy, bypass, ref request_id);

                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    //_logger.LogWarning(JsonConvert.SerializeObject(resultRsp));
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(resultRsp.reply);

                        return JsonConvert.DeserializeObject<DishGroupResponse>(jsonStr).dish_groups;
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
                _logger.LogWarning("===> GetDishGroupShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public List<DishesShopee> GetDishShopee(string url,string app_id, string app_key, int restaurant_id, string proxy, string[] bypass ,ref string message)
        {
            try
            {
                url = url + "?restaurant_id=" + restaurant_id;
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("GET", url, null));
                var result = ShopeeApiHelper.GetApi(url, ShopeeConst.ShopeeVersion, app_id, sign, proxy, bypass, ref request_id);

                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    //_logger.LogWarning(JsonConvert.SerializeObject(resultRsp));
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(resultRsp.reply);

                        return JsonConvert.DeserializeObject<DishGetResponse>(jsonStr).dishes;
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
                _logger.LogWarning("===> GetDishShopee.Exception: " + ex.Message.ToString());
                return null;
            }

        }

        public List<DishGetOutOfServiceResult> GetOutOfServiceShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishGetOutOfServiceRequest request, ref string message)
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
                        return JsonConvert.DeserializeObject<List<DishGetOutOfServiceResult>>(JsonConvert.SerializeObject(resultRsp.reply));
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
                _logger.LogWarning("===> IDishShopeeService.GetOutOfServiceShopee.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public Get_approval_status_rsp Get_approval_status(string url, string app_id, string app_key, string proxy, string[] bypass, Get_approval_status request, ref string message)
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
                        return JsonConvert.DeserializeObject<Get_approval_status_rsp>(JsonConvert.SerializeObject(resultRsp.reply));
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
                _logger.LogWarning("===> IDishShopeeService.dish_upload_picture.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public List<DishGroupsInfo> Get_dish_partner_group_id_mappings(string url, string app_id, string app_key, int restaurant_id, string proxy, string[] bypass, ref string message)
        {
            try
            {
                url = url + "?restaurant_id=" + restaurant_id;
                string sign = ShopeeUtils.CreateSignature(app_key, ShopeeUtils.CreateBaseString("GET", url, null));
                var result = ShopeeApiHelper.GetApi(url, ShopeeConst.ShopeeVersion, app_id, sign, proxy, bypass, ref request_id);

                if (!string.IsNullOrEmpty(result))
                {
                    var resultRsp = JsonConvert.DeserializeObject<ResultShopeeRsp>(result);
                    _logger.LogWarning(JsonConvert.SerializeObject(resultRsp));
                    message = resultRsp.result.ToString();
                    if (resultRsp.result == ResultCodeShopee.success.ToString())
                    {
                        string jsonStr = JsonConvert.SerializeObject(resultRsp.reply);

                        return JsonConvert.DeserializeObject<dish_partner_group_id_mappings_response>(jsonStr).dish_groups;
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
                _logger.LogWarning("===> get_partner_group_id_mappings.Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public bool MappingDishShopee(string url, string app_id, string app_key, DishMappingsRequest request, string proxy, string[] bypass, ref string message)
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
                _logger.LogWarning("===> GetToppingGroupShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool UpdateDishBulkShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishBulkCreateRequest request, ref string message)
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
                _logger.LogWarning("===> UpdateDishBulkShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool UpdateDishGroupShopee(string url, string app_id, string app_key, string proxy, string[] bypass, DishGroupCreateRequest request, ref string message)
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
                _logger.LogWarning("===> UpdateDishGroupShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool UpdateDishShopee(string url, string app_id, string app_key, string proxy, string[] bypass, object request, ref string message)
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
                _logger.LogWarning("===> UpdateDishShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public bool UpdatePartneGroupIdMappingShopee(string url, string app_id, string app_key, string proxy, string[] bypass, Dish_update_partner_group_id_mappings request, ref string message)
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
                _logger.LogWarning("===> UpdatePartneGroupIdMappingShopee.Exception: " + ex.Message.ToString());
                return false;
            }
        }
    }
}
