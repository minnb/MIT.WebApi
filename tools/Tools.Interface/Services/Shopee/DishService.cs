using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tools.Interface.Database;
using Tools.Interface.Dtos;
using Tools.Interface.Helpers;
using Tools.Interface.Models;
using VCM.Common.Helpers;
using VCM.Shared.API.Shopee;
using VCM.Shared.API.Shopee.Dish;
using VCM.Shared.Entity.Central;
using VCM.Shared.Entity.SalesPartner;

namespace Tools.Interface.Services.Shopee
{
    public class DishService
    {
        private readonly InterfaceEntry _interfaceEntry;
        private DishRepository _dishRepository;
        public DishService
            (
                InterfaceEntry interfaceEntry
            )
        {
            _interfaceEntry = interfaceEntry;
            _dishRepository = new DishRepository(_interfaceEntry.Prefix);
        }
        //Dish group
        public void GetShopeeDishGroupByStore(string job_name, WebApiInfo webApiInfo, string storeNo)
        {
            var routerData = webApiInfo.WebRoute.Where(x => x.Name == "GetShopeeDishGroup").FirstOrDefault();
            var router = routerData.Route + "?StoreNo=" + storeNo;
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
                var dishGroupShopee = JsonConvert.DeserializeObject<List<DishGroupShopee>>(JsonConvert.SerializeObject(result.data));
                _dishRepository.SyncDishGroup(job_name, dishGroupShopee, storeNo);
            }
        }
        public void GetShopeeDishGroup(string job_name)
        {
            try
            {
                var lstStore = _dishRepository.GetShopeeRestaurant(job_name);
                if(lstStore != null && lstStore.Count > 0)
                {
                    var webApiInfo = _dishRepository.GetWebApiInfo(job_name);
                    if (webApiInfo == null)
                    {
                        return;
                    }
                    foreach (var store in lstStore)
                    {
                        GetShopeeDishGroupByStore(job_name, webApiInfo, store.partner_restaurant_id);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> GetShopeeDishGroup Exception: " + ex.Message.ToString());
            }
        }
        //Dish
        public void GetDishByStore(string job_name,WebApiInfo webApiInfo, string storeNo)
        {
            if (webApiInfo == null)
            {
                return;
            }
            var routerData = webApiInfo.WebRoute.Where(x => x.Name == "GetShopeeDish").FirstOrDefault();
            var router = routerData.Route + "?StoreNo=" + storeNo;
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
                var dishShopee = JsonConvert.DeserializeObject<List<DishesShopee>>(JsonConvert.SerializeObject(result.data));
                _dishRepository.SyncDish(job_name, dishShopee, storeNo);
            }
        }
        public void GetShopeeDish(string job_name)
        {
            try
            {
                var lstStore = _dishRepository.GetShopeeRestaurant(job_name);
                if(lstStore != null && lstStore.Count > 0)
                {
                    var webApiInfo = _dishRepository.GetWebApiInfo(job_name);
                    foreach (var store in lstStore)
                    {
                        GetDishByStore(job_name, webApiInfo, store.partner_restaurant_id);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> GetShopeeDish Exception: " + ex.Message.ToString());
            }
        }
        //Create dish
        public void CreateShopeeDish(string job_name)
        {
            try
            {
                var webApiInfo = _dishRepository.GetWebApiInfo(job_name);
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                var itemSalesOnApps = _dishRepository.GetDishCreate(job_name);
                if(itemSalesOnApps == null)
                {
                    return;
                }
                else if(itemSalesOnApps.Count() > 0)
                {
                    foreach (var item in itemSalesOnApps)
                    {
                        if(_dishRepository.lstStautsBlock.Contains(item.Action))
                        {
                            Set_StatusesShopeeDish(job_name, webApiInfo, item);
                        }
                        else if (_dishRepository.lstStautsUpdateInfo.Contains(item.Action) && item.IsSales == true)
                        {
                            UpdateShopeeDish(job_name, webApiInfo, item);
                        }
                        else if(item.IsSales == true && item.IsSync == false && item.Blocked == false && item.Action == "CREATE")
                        {
                            var routerData = webApiInfo.WebRoute.Where(x => x.Name == "CreateShopeeDish").FirstOrDefault();
                            var router = routerData.Route;
                            FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                            FileHelper.Write2Logs(job_name, "===> CreateShopeeDish request: " + JsonConvert.SerializeObject(ShopeeMappingHelper.MappingDishCreateRequest(item)));
                            var api = new RestApiHelper(
                                        job_name,
                                        webApiInfo.Host,
                                        router,
                                        "POST",
                                        null,
                                        null,
                                        ShopeeMappingHelper.MappingDishCreateRequest(item)
                                   );
                            var strResponse = api.InteractWithApi();
                            Console.WriteLine("CreateShopeeDish ItemName: {0}", item.ItemNo + "-" +item.ItemName);

                            var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                            if (result.meta.code == 200)
                            {
                                item.SyncResults = result.meta.message ?? "Success";
                                item.IsSync = true;
                                FileHelper.Write2Logs(job_name, "===> CreateShopeeDish result: " + JsonConvert.SerializeObject(result));
                            }
                            else
                            {
                                var mess = result.meta.message;
                                var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                                item.SyncResults = resultShopee.result ?? "Failed";
                                if (resultShopee.result == "error_partner_id_existed")
                                {
                                    item.IsSync = true;
                                }
                            }
                            _dishRepository.UpdateStatusDishCreate(job_name, item, true);
                        }
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> CreateShopeeDish Exception: " + ex.Message.ToString());
            }
        }
        public void Set_StatusesShopeeDish(string job_name, WebApiInfo webApiInfo, ListItemSalesOnApp item)
        {
            //AVAILABLE=1
            //OUT_OF_STOCK=2 I
            //NACTIVE = 3
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                var routerData = webApiInfo.WebRoute.Where(x => x.Name == "Set_StatusesShopeeDish").FirstOrDefault();
                var router = routerData.Route;
                FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                //get data
                if (item != null)
                {
                    int status = 1;
                    if(item.Action.ToUpper() == "BLOCKED")
                    {
                        status = 3;
                    }
                    else if (item.Action.ToUpper() == "ACTIVE")
                    {
                        status = 1;
                    }
                    List<ArrDishSetStatuses> lstDish = new List<ArrDishSetStatuses>
                    {
                        new ArrDishSetStatuses()
                        {
                            partner_dish_id = item.ItemNo,
                            status = status

                        }
                    };
                    var bodyData = new DishSetStatusesRequest()
                    {
                        restaurant_id = item.StoreId,
                        partner_restaurant_id = item.StoreNo,
                        is_apply_all = item.IsApplyAll,
                        dishes = lstDish
                    };

                    FileHelper.Write2Logs(job_name, "===> Set_StatusesShopeeDish request: " + JsonConvert.SerializeObject(bodyData));
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
                    Console.WriteLine("Set_StatusesShopeeDish: {0}", item.ItemNo);

                    var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                    if (result.meta.code == 200)
                    {
                        item.SyncResults = result.meta.message ?? "Success";
                        item.IsSync = true;
                        FileHelper.Write2Logs(job_name, "===> Set_StatusesShopeeDish result: " + JsonConvert.SerializeObject(result));
                    }
                    else
                    {
                        var mess = result.meta.message;
                        var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                        if (resultShopee.result == "error_partner_id_existed" || resultShopee.result == "error_dish_not_found")
                        {
                            item.IsSync = true;
                        }
                        else
                        {
                            item.IsSync = false;
                        }
                        item.SyncResults = resultShopee.result ?? "Failed";
                    }
                    _dishRepository.UpdateStatusActionLogging(job_name, item.Id, false,item.Action, item.IsSync, item.SyncResults);
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> Set_StatusesShopeeDish Exception: " + ex.Message.ToString());
            }
        }
        public void UpdateShopeeDish(string job_name, WebApiInfo webApiInfo, ListItemSalesOnApp item)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                var routerData = webApiInfo.WebRoute.Where(x => x.Name == "UpdateShopeeDish").FirstOrDefault();
                var router = routerData.Route;
                FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                //get data
                if (item != null)
                {
                    FileHelper.Write2Logs(job_name, "===> UpdateShopeeDish request: " + JsonConvert.SerializeObject(ShopeeMappingHelper.MappingDishCreateRequest(item)));
                    var api = new RestApiHelper(
                                job_name,
                                webApiInfo.Host,
                                router,
                                "POST",
                                null,
                                null,
                                ShopeeMappingHelper.MappingDishCreateRequest(item)
                           );
                    var strResponse = api.InteractWithApi();
                    Console.WriteLine("UpdateShopeeDish: {0}", item.ItemNo);

                    var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                    if (result.meta.code == 200)
                    {
                        item.SyncResults = result.meta.message ?? "Success";
                        item.IsSync = true;
                        FileHelper.Write2Logs(job_name, "===> UpdateShopeeDish result: " + JsonConvert.SerializeObject(result));
                    }
                    else
                    {
                        var mess = result.meta.message;
                        item.IsSync = false;
                        var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                        item.SyncResults = resultShopee.result ?? "Failed";
                    }
                    _dishRepository.UpdateStatusActionLogging(job_name, item.Id, false, item.Action, item.IsSync, item.SyncResults);
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateShopeeDish Exception: " + ex.Message.ToString());
            }
        }

        //Dish toppig mapping create
        public void DishToppingMappingCreate(string job_name)
        {
            try
            {
                var webApiInfo = _dishRepository.GetWebApiInfo(job_name);
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                var data = _dishRepository.DishToppingMappingCreate(job_name);
                if (data == null)
                {
                    return;
                }
                else if(data.Count > 0)
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "DishToppingMappingCreate").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);

                    foreach (var item in data)
                    {
                        if(item.Blocked == true)
                        {
                            DishToppingMappingDelete(job_name, webApiInfo, item);
                        }

                        FileHelper.Write2Logs(job_name, "===> DishToppingMappingCreate request: " + JsonConvert.SerializeObject(ShopeeMappingHelper.MappingDishToppingCreateRequest(item)));
                        var api = new RestApiHelper(
                                    job_name,
                                    webApiInfo.Host,
                                    router,
                                    "POST",
                                    null,
                                    null,
                                    ShopeeMappingHelper.MappingDishToppingCreateRequest(item)
                               );
                        var strResponse = api.InteractWithApi();
                        Console.WriteLine("DishToppingMappingCreate: {0} - {1}", item.ToppingNo, item.ItemNo);
                        var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);

                        if (result.meta.code == 200)
                        {
                            item.SyncResults = result.meta.message ?? "Success";
                            item.IsSync = true;
                            FileHelper.Write2Logs(job_name, "===> DishToppingMappingCreate result: " + JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(result.meta.message);
                            item.SyncResults = resultShopee.result ?? "Failed";
                            if (item.SyncResults == "error_partner_id_existed")
                            {
                                item.IsSync = true;
                            }
                        }
                        _dishRepository.UpdateStatusDishToppingMappingCreate(job_name, item, true);
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> DishToppingMappingCreate Exception: " + ex.Message.ToString());
            }
        }
        public void DishToppingSetGroupQuantity(string job_name)
        {
            try
            {
                var webApiInfo = _dishRepository.GetWebApiInfo(job_name);
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                var data = _dishRepository.DishToppingMappingCreate(job_name, true);
                if (data == null)
                {
                    return;
                }
                else
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "ToppingSetGroupQuantity").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);

                    foreach (var item in data)
                    {
                        FileHelper.Write2Logs(job_name, "===> ToppingSetGroupQuantity request: " + JsonConvert.SerializeObject(ShopeeMappingHelper.MappingDishToppingCreateRequest(item)));
                        var api = new RestApiHelper(
                                    job_name,
                                    webApiInfo.Host,
                                    router,
                                    "POST",
                                    null,
                                    null,
                                    ShopeeMappingHelper.MappingToppingSetGroupQuantityRequest(item)
                               );
                        var strResponse = api.InteractWithApi();
                        Console.WriteLine("ToppingSetGroupQuantity: {0}", item.ItemNo);

                        var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                        if (result.meta.code == 200)
                        {
                            item.SyncSetQuantity = result.meta.message ?? "Success";
                            item.IsSetQuantity = true;
                            FileHelper.Write2Logs(job_name, "===> ToppingSetGroupQuantity result: " + JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(result.meta.message);
                            item.SyncSetQuantity = resultShopee.result ?? "Failed";
                            if (item.SyncSetQuantity == "error_partner_id_existed")
                            {
                                item.IsSetQuantity = true;
                            }
                        }
                        _dishRepository.UpdateStatusSetQuantityTopping(job_name, item, true);
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> ToppingSetGroupQuantity Exception: " + ex.Message.ToString());
            }
        }
        public void DishToppingMappingDelete(string job_name, WebApiInfo webApiInfo, ListItemToppingMapping item)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                var routerData = webApiInfo.WebRoute.Where(x => x.Name == "DishToppingMappingDelete").FirstOrDefault();
                var router = routerData.Route;
                FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                var bodyData = new DishDeleteToppingMappingRequest()
                {
                    restaurant_id = item.StoreId,
                    partner_dish_id = item.ItemNo,
                    partner_restaurant_id = item.StoreNo,
                    partner_topping_ids = new string[] { item.ToppingNo }
                };
                //get data
                if (item != null)
                {
                    var api = new RestApiHelper(
                                job_name,
                                webApiInfo.Host,
                                router,
                                "POST",
                                null,
                                null,
                                bodyData
                           );
                    FileHelper.Write2Logs(job_name, "===> DishToppingMappingDelete request: " + JsonConvert.SerializeObject(bodyData));
                    var strResponse = api.InteractWithApi();
                    Console.WriteLine("DishToppingMappingDelete: {0}", item.ItemNo);

                    var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                    if (result.meta.code == 200)
                    {
                        item.SyncResults = "DishToppingMappingDelete: " + result.meta.message ?? "Success";
                        item.IsSync = true;
                        FileHelper.Write2Logs(job_name, "===> DishToppingMappingDelete result: " + JsonConvert.SerializeObject(result));
                    }
                    else
                    {
                        var mess = result.meta.message;
                        var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                        item.SyncResults = resultShopee.result ?? "Failed";
                        if (resultShopee.result == "error_partner_id_existed")
                        {
                            item.IsSync = true;
                        }
                    }
                    _dishRepository.UpdateStatusDishToppingMappingCreate(job_name, item, true);
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> DishToppingMappingDelete Exception: " + ex.Message.ToString());
            }
        }
    }
}
