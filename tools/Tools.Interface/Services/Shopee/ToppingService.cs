using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tools.Interface.Dtos;
using Tools.Interface.Helpers;
using Tools.Interface.Models;
using VCM.Common.Helpers;
using VCM.Shared.API.Shopee;
using VCM.Shared.API.Shopee.Topping;
using VCM.Shared.Entity.Central;
using VCM.Shared.Entity.SalesPartner;

namespace Tools.Interface.Services.Shopee
{
    public class ToppingService
    {
        private readonly InterfaceEntry _interfaceEntry;
        private ToppingRepository _toppingRepository;
        public ToppingService
            (
                InterfaceEntry interfaceEntry
            )
        {
            _interfaceEntry = interfaceEntry;
            _toppingRepository = new ToppingRepository(_interfaceEntry.Prefix);
        }
        public void GetToppingByStore(string job_name, WebApiInfo webApiInfo, string StoreNo)
        {
            if (webApiInfo == null)
            {
                return;
            }
            var routerData = webApiInfo.WebRoute.Where(x => x.Name == "GetShopeeTopping").FirstOrDefault();
            var router = routerData.Route + "?StoreNo=" + StoreNo;

            FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
            var api = new RestApiHelper(
                        job_name,
                        webApiInfo.Host,
                        router,
                        "GET",
                        null,
                        null,
                        null
                   );
            var strResponse = api.InteractWithApi();
            var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
            if (result.meta.code == 200)
            {
                var dataSync = JsonConvert.DeserializeObject<List<toppings>>(JsonConvert.SerializeObject(result.data));
                _toppingRepository.SyncTopping(job_name, dataSync, StoreNo);
            }

        }
        public void GetShopeeTopping(string job_name)
        {
            try
            {
                var lstStore = _toppingRepository.GetShopeeRestaurant(job_name);
                if (lstStore != null && lstStore.Count > 0)
                {
                    var webApiInfo = _toppingRepository.GetWebApiInfo(job_name);
                    foreach(var store in lstStore)
                    {
                        GetToppingByStore(job_name, webApiInfo, store.partner_restaurant_id);
                    }
                }              
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> ImportTransToDB Exception: " + ex.Message.ToString());
            }
        }
        public void GetShopeeToppingGroupByStore(string job_name, WebApiInfo webApiInfo, string StoreNo)
        {
            var routerData = webApiInfo.WebRoute.Where(x => x.Name == "GetShopeeToppingGroup").FirstOrDefault();
            var router = routerData.Route + "?StoreNo=" + StoreNo;
            FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
            var api = new RestApiHelper(
                        job_name,
                        webApiInfo.Host,
                        router,
                        "GET",
                        null,
                        null,
                        null
                   );
            var strResponse = api.InteractWithApi();
            var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
            if (result.meta.code == 200)
            {
                var dataSync = JsonConvert.DeserializeObject<List<get_topping_groups>>(JsonConvert.SerializeObject(result.data));
                _toppingRepository.SyncToppingGroup(job_name, dataSync, StoreNo);
            }
        }
        public void GetShopeeToppingGroup(string job_name)
        {
            try
            {
                var lstStore = _toppingRepository.GetShopeeRestaurant(job_name);
                if (lstStore != null && lstStore.Count > 0)
                {
                    var webApiInfo = _toppingRepository.GetWebApiInfo(job_name);
                    if (webApiInfo == null)
                    {
                        return;
                    }
                    foreach(var store in lstStore)
                    {
                        GetShopeeToppingGroupByStore(job_name, webApiInfo, store.partner_restaurant_id);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> ImportTransToDB Exception: " + ex.Message.ToString());
            }
        }
        public void CreateShopeeTopping(string job_name)
        {
            try
            {
                var webApiInfo = _toppingRepository.GetWebApiInfo(job_name);
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                var itemSalesOnApps = _toppingRepository.GetToppingCreate(job_name);
                if (itemSalesOnApps == null)
                {
                    return;
                }
                else if(itemSalesOnApps.Count > 0)
                {
                    foreach (var item in itemSalesOnApps)
                    {
                        if (item.Blocked == true)
                        {
                            DeleteShopeeTopping(job_name, webApiInfo, item);
                        }
                        if (_toppingRepository.lstStautsBlock.Contains(item.Action))
                        {
                            SetStatusesShopeeTopping(job_name, webApiInfo, item);
                        }
                        else if (_toppingRepository.lstStautsUpdatePrice.Contains(item.Action))
                        {
                            UpdatePriceShopeeTopping(job_name, webApiInfo, item);
                        }
                        else if (_toppingRepository.lstStautsUpdateInfo.Contains(item.Action))
                        {
                            UpdateShopeeTopping(job_name, webApiInfo, item);
                        }
                        else if(item.Action == "CREATE" && item.IsSync == false && item.Blocked == false)
                        {
                            var routerData = webApiInfo.WebRoute.Where(x => x.Name == "CreateShopeeTopping").FirstOrDefault();
                            var router = routerData.Route;
                            FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                            FileHelper.Write2Logs(job_name, "===> CreateShopeeTopping request: " + JsonConvert.SerializeObject(ShopeeMappingHelper.MappingToppingCreateRequest(item)));
                            var api = new RestApiHelper(
                                        job_name,
                                        webApiInfo.Host,
                                        router,
                                        "POST",
                                        null,
                                        null,
                                        ShopeeMappingHelper.MappingToppingCreateRequest(item)
                                   );
                            var strResponse = api.InteractWithApi();
                            Console.WriteLine("CreateShopeeTopping ItemName: {0}", item.ToppingNo + "-" + item.Name + "-" + item.ToppingGroup);

                            var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                            if (result.meta.code == 200)
                            {
                                item.SyncResults = result.meta.message ?? "Success";
                                item.IsSync = true;
                                FileHelper.Write2Logs(job_name, "===> CreateShopeeTopping result: " + JsonConvert.SerializeObject(result));
                            }
                            else
                            {
                                var mess = result.meta.message;
                                var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                                item.SyncResults = resultShopee.result ?? "Failed";
                                if (item.SyncResults == "error_partner_id_existed")
                                {
                                    item.IsSync = true;
                                }
                            }
                            _toppingRepository.UpdateStatusToppingCreate(job_name, item, true);
                        }
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> CreateShopeeTopping Exception: " + ex.Message.ToString());
            }
        }
        public void UpdateShopeeTopping(string job_name, WebApiInfo webApiInfo, ListToppingSalesOnApp item)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                if (item == null)
                {
                    return;
                }
                else
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "UpdateShopeeTopping").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    FileHelper.Write2Logs(job_name, "===> UpdateShopeeTopping request: " + JsonConvert.SerializeObject(ShopeeMappingHelper.MappingToppingCreateRequest(item)));
                    var api = new RestApiHelper(
                                job_name,
                                webApiInfo.Host,
                                router,
                                "POST",
                                null,
                                null,
                                ShopeeMappingHelper.MappingToppingCreateRequest(item)
                           );
                    var strResponse = api.InteractWithApi();
                    Console.WriteLine("UpdateShopeeTopping ItemName: {0}", item.ToppingNo + "-" + item.Name + "-" + item.ToppingGroup);

                    var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                    if (result.meta.code == 200)
                    {
                        item.SyncResults = result.meta.message ?? "Success";
                        item.IsSync = true;
                        FileHelper.Write2Logs(job_name, "===> UpdateShopeeTopping result: " + JsonConvert.SerializeObject(result));
                    }
                    else
                    {
                        var mess = result.meta.message;
                        var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                        item.SyncResults = resultShopee.result ?? "Failed";
                        if (item.SyncResults == "error_partner_id_existed")
                        {
                            item.IsSync = true;
                        }
                        else
                        {
                            item.IsSync = false;
                        }
                    }
                    _toppingRepository.UpdateStatusActionLogging(job_name, item.Id, false, item.Action, item.IsSync, item.SyncResults);
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateShopeeTopping Exception: " + ex.Message.ToString());
            }
        }
        public void DeleteShopeeTopping(string job_name, WebApiInfo webApiInfo, ListToppingSalesOnApp item)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                if (item == null)
                {
                    return;
                }
                else 
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "DeleteShopeeTopping").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    FileHelper.Write2Logs(job_name, "===> DeleteShopeeTopping request: " + JsonConvert.SerializeObject(ShopeeMappingHelper.MappingToppingCreateRequest(item)));
                    var api = new RestApiHelper(
                                job_name,
                                webApiInfo.Host,
                                router,
                                "POST",
                                null,
                                null,
                                new topping_delete()
                                {
                                    partner_restaurant_id = item.StoreNo,
                                    restaurant_id = item.StoreId,
                                    partner_topping_id = item.ToppingNo
                                }
                           );
                    var strResponse = api.InteractWithApi();
                    Console.WriteLine("DeleteShopeeTopping: {0}", item.ToppingNo + "-" + item.Name + "-" + item.ToppingGroup);

                    var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                    if (result.meta.code == 200)
                    {
                        item.SyncResults = "DeleteShopeeTopping:" + result.meta.message ?? "Success";
                        item.IsSync = true;
                        FileHelper.Write2Logs(job_name, "===> CreateShopeeTopping result: " + JsonConvert.SerializeObject(result));
                    }
                    else
                    {
                        var mess = result.meta.message;
                        var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                        item.SyncResults = "DeleteShopeeTopping:" + resultShopee.result ?? "Failed";
                        if (resultShopee.result == "error_partner_id_existed" || resultShopee.result == "error_topping_not_found")
                        {
                            item.IsSync = true;
                        }
                        else
                        {
                            item.IsSync = false;
                        }
                    }
                    _toppingRepository.UpdateStatusActionLogging(job_name, item.Id, false, item.Action, item.IsSync, item.SyncResults);
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> DeleteShopeeTopping Exception: " + ex.Message.ToString());
            }
        }
        public void UpdatePriceShopeeTopping(string job_name, WebApiInfo webApiInfo, ListToppingSalesOnApp item)
        {
            try
            {
                //get data
                if (item == null)
                {
                    return;
                }
                else
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "UpdatePriceShopeeTopping").FirstOrDefault();
                    if (routerData == null) return;

                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    List<List_topping_update_prices> topping = new List<List_topping_update_prices>();
                    string StoreNo = "ALL";
                    if(item.IsApplyAll == false)
                    {
                        StoreNo = item.StoreNo;
                    }
                    var dishes = _toppingRepository.GetDishPartner_From_ToppingPartner(item.ToppingNo, StoreNo);
                    if(dishes.Count > 0)
                    {
                        foreach(var d in dishes)
                        {
                            topping.Add(new List_topping_update_prices()
                            {
                                partner_topping_id = item.ToppingNo,
                                price = item.UnitPrice,
                                partner_dish_id = d.ItemNo
                            });
                        }

                    }
                    var bodyData = new topping_update_prices()
                    {
                        is_apply_all = item.IsApplyAll,
                        restaurant_id = item.StoreId,
                        partner_restaurant_id = item.StoreNo,
                        partner_topping_group_id = item.ToppingGroup,
                        toppings = topping
                    };
                    FileHelper.Write2Logs(job_name, "===> UpdateShopeeTopping request: " + JsonConvert.SerializeObject(bodyData));
                    var api = new RestApiHelper(
                                job_name,
                                webApiInfo.Host,
                                router,
                                "POST",
                                null,
                                null,
                                bodyData
                           );
                    var strResponse = api.InteractWithApi();
                    Console.WriteLine("UpdatePriceShopeeTopping ItemName: {0}", item.ToppingNo + "-" + item.Name + "-" + item.ToppingGroup);

                    var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                    if (result.meta.code == 200)
                    {
                        item.SyncResults = result.meta.message ?? "Success";
                        item.IsSync = true;
                        FileHelper.Write2Logs(job_name, "===> UpdatePriceShopeeTopping result: " + JsonConvert.SerializeObject(result));
                        _toppingRepository.UpdateItemToppingMapping(job_name, dishes);
                    }
                    else
                    {
                        var mess = result.meta.message;
                        var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                        item.SyncResults = resultShopee.result ?? "Failed";
                        if (item.SyncResults == "error_partner_id_existed")
                        {
                            item.IsSync = true;
                        }
                        else
                        {
                            item.IsSync = false;
                        }
                    }
                    _toppingRepository.UpdateStatusActionLogging(job_name, item.Id, false, item.Action, item.IsSync, item.SyncResults);
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdatePriceShopeeTopping Exception: " + ex.Message.ToString());
            }
        }
        public void SetStatusesShopeeTopping(string job_name, WebApiInfo webApiInfo, ListToppingSalesOnApp item)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                if (item == null)
                {
                    return;
                }
                else
                {
                    int status = 1;
                    if (item.Action.ToUpper() == "BLOCKED")
                    {
                        status = 3;
                    }
                    else if (item.Action.ToUpper() == "ACTIVE")
                    {
                        status = 1;
                    }
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "SetStatusesShopeeTopping").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    List<toppings_set_status> topping = new List<toppings_set_status>();
                    topping.Add(new toppings_set_status()
                    {
                        partner_topping_id = item.ToppingNo,
                        status = status
                    });
                    var bodyData = new topping_set_statuses()
                    {
                        restaurant_id = item.StoreId,
                        partner_restaurant_id = item.StoreNo,
                        is_apply_all = item.IsApplyAll,
                        toppings = topping
                    };
                    FileHelper.Write2Logs(job_name, "===> SetStatusesShopeeTopping request: " + JsonConvert.SerializeObject(bodyData));
                    var api = new RestApiHelper(
                                job_name,
                                webApiInfo.Host,
                                router,
                                "POST",
                                null,
                                null,
                                bodyData
                           );
                    var strResponse = api.InteractWithApi();
                    Console.WriteLine("SetStatusesShopeeTopping ItemName: {0}", item.ToppingNo + "-" + item.Name + "-" + item.ToppingGroup);

                    var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                    if (result.meta.code == 200)
                    {
                        item.SyncResults = result.meta.message ?? "Success";
                        item.IsSync = true;
                        FileHelper.Write2Logs(job_name, "===> UpdatePriceShopeeTopping result: " + JsonConvert.SerializeObject(result));
                    }
                    else
                    {
                        var mess = result.meta.message;
                        var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                        item.SyncResults = resultShopee.result ?? "Failed";
                        item.IsSync = false;
                    }
                    _toppingRepository.UpdateStatusActionLogging(job_name, item.Id, false, item.Action, item.IsSync, item.SyncResults);
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> SetStatusesShopeeTopping Exception: " + ex.Message.ToString());
            }
        }
    }
}
