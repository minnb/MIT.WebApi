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
using VCM.Shared.API.Shopee.Dish;
using VCM.Shared.API.Shopee.Topping;
using VCM.Shared.Entity.Central;
using VCM.Shared.Entity.SalesPartner;

namespace Tools.Interface.Services.Shopee
{
    public class MappingService
    {
        private readonly InterfaceEntry _interfaceEntry;
        private MappingRepository _mappingRepository;
        public MappingService
            (
                InterfaceEntry interfaceEntry
            )
        {
            _interfaceEntry = interfaceEntry;
            _mappingRepository = new MappingRepository(_interfaceEntry.Prefix);
        }
        public void RunMappingPartner(string job_name)
        {
            var webApiInfo = _mappingRepository.GetWebApiInfo(job_name);
            var dataMapping = _mappingRepository.GetMappingDataPartner(job_name);
            if(dataMapping == null)
            {
                return;
            }
            else
            {
                MappingDishPartner(job_name, webApiInfo, dataMapping);
                Thread.Sleep(100);
                MappingDishGroupPartner(job_name, webApiInfo, dataMapping);
                Thread.Sleep(100);
                MappingToppingPartner(job_name, webApiInfo, dataMapping);
                Thread.Sleep(100);
                MappingToppingGroupPartner(job_name, webApiInfo, dataMapping);
                Thread.Sleep(100);
                MappingBlockShopeeDish(job_name, webApiInfo, dataMapping);
            }
        }
        private void MappingDishPartner(string job_name, WebApiInfo webApiInfo, List<ListMappingDataPartner> data)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                var dataMapping = data.Where(x => x.Action.ToUpper() == "MAPPING_DISH").ToList();
                if (dataMapping == null)
                {
                    return;
                }
                else if (dataMapping.Count > 0)
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "MappingDishPartner").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    foreach (var item in dataMapping)
                    {
                        List<DishesMapping> dishes = new List<DishesMapping>
                        {
                            new DishesMapping()
                            {
                                dish_id = item.RefId,
                                partner_dish_id = item.ItemNo
                            }
                        };
                        var dataBody = new DishMappingsRequest()
                        {
                            restaurant_id = item.StoreId,
                            dishes = dishes
                        };

                        FileHelper.Write2Logs(job_name, "===> MappingDishPartner request: " + JsonConvert.SerializeObject(dataBody));
                        var api = new RestApiHelper(
                                    job_name,
                                    webApiInfo.Host,
                                    router,
                                    "POST",
                                    null,
                                    null,
                                    dataBody
                               );
                        var strResponse = api.InteractWithApi();
                        Console.WriteLine("MappingDishPartner ItemName: {0}", item.ItemNo + "-" + item.RefId.ToString());

                        var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                        if (result.meta.code == 200)
                        {
                            item.Description = result.meta.message ?? "Success";
                            item.IsSync = true;
                            FileHelper.Write2Logs(job_name, "===> MappingDishPartner result: " + JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            var mess = result.meta.message;
                            var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                            item.Description = resultShopee.result ?? "Failed";
                            if (resultShopee.result == "error_partner_id_existed")
                            {
                                item.IsSync = true;
                            }
                        }
                        _mappingRepository.UpdateStatusMapping(job_name, item);
                        Thread.Sleep(100);
                    }

                    Thread.Sleep(200);
                    DishService dishService = new DishService(_interfaceEntry);
                    var lstStore = dataMapping
                              .Select(x => new
                              {
                                  x.StoreNo
                              })
                              .GroupBy(x => new { x.StoreNo })
                              .Select(x =>
                              {
                                  var temp = x.OrderByDescending(o => o.StoreNo).FirstOrDefault();
                                  return new
                                  {

                                      x.Key.StoreNo
                                  };
                              }).ToList();
                    foreach (var store in lstStore)
                    {
                        dishService.GetDishByStore(job_name, webApiInfo, store.StoreNo);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> MappingDishPartner Exception: " + ex.Message.ToString());
            }
        }
        private void MappingDishGroupPartner(string job_name, WebApiInfo webApiInfo, List<ListMappingDataPartner> data)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                var dataMapping = data.Where(x => x.Action.ToUpper() == "MAPPING_DISH_GR").ToList();
                if (dataMapping == null)
                {
                    return;
                }
                else if (dataMapping.Count > 0)
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "MappingDishGroupPartner").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    foreach (var item in dataMapping)
                    {
                        List<DishGroupUpdatePartnerMapping> dish_groups = new List<DishGroupUpdatePartnerMapping>
                        {
                            new DishGroupUpdatePartnerMapping()
                            {
                                dish_group_id = item.RefId,
                                partner_dish_group_id = item.ItemNo
                            }
                        };
                        var dataBody = new Dish_update_partner_group_id_mappings()
                        {
                            restaurant_id = item.StoreId,
                            dish_groups = dish_groups
                        };

                        FileHelper.Write2Logs(job_name, "===> MappingDishGroupPartner request: " + JsonConvert.SerializeObject(dataBody));
                        var api = new RestApiHelper(
                                    job_name,
                                    webApiInfo.Host,
                                    router,
                                    "POST",
                                    null,
                                    null,
                                    dataBody
                               );
                        var strResponse = api.InteractWithApi();
                        Console.WriteLine("MappingDishGroupPartner ItemName: {0}", item.ItemNo + "-" + item.RefId.ToString());

                        var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                        if (result.meta.code == 200)
                        {
                            item.Description = result.meta.message ?? "Success";
                            item.IsSync = true;
                            FileHelper.Write2Logs(job_name, "===> MappingDishGroupPartner result: " + JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            var mess = result.meta.message;
                            var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                            item.Description = resultShopee.result ?? "Failed";
                            if (resultShopee.result == "error_partner_id_existed")
                            {
                                item.IsSync = true;
                            }
                        }
                        _mappingRepository.UpdateStatusMapping(job_name, item);
                        Thread.Sleep(100);
                    }

                    Thread.Sleep(200);
                    DishService dishService = new DishService(_interfaceEntry);
                    var lstStore = dataMapping
                              .Select(x => new
                              {
                                  x.StoreNo
                              })
                              .GroupBy(x => new { x.StoreNo })
                              .Select(x =>
                              {
                                  var temp = x.OrderByDescending(o => o.StoreNo).FirstOrDefault();
                                  return new
                                  {

                                      x.Key.StoreNo
                                  };
                              }).ToList();
                    foreach (var store in lstStore)
                    {
                        dishService.GetShopeeDishGroupByStore(job_name, webApiInfo, store.StoreNo);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> MappingDishGroupPartner Exception: " + ex.Message.ToString());
            }
        }
        public void MappingToppingPartner(string job_name, WebApiInfo webApiInfo, List<ListMappingDataPartner> data)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                var dataMapping = data.Where(x => x.Action.ToUpper() == "MAPPING_TOPPING").ToList();
                if (dataMapping == null)
                {
                    return;
                }
                else if (dataMapping.Count > 0)
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "MappingToppingPartner").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    foreach (var item in dataMapping)
                    {
                        List<ToppingMapping> toppings = new List<ToppingMapping>
                        {
                            new ToppingMapping()
                            {
                                topping_id = item.RefId,
                                partner_topping_id = item.ItemNo
                            }
                        };
                        var dataBody = new ToppingMappingRequest()
                        {
                            restaurant_id = item.StoreId,
                            toppings = toppings
                        };

                        FileHelper.Write2Logs(job_name, "===> MappingToppingPartner request: " + JsonConvert.SerializeObject(dataBody));
                        var api = new RestApiHelper(
                                    job_name,
                                    webApiInfo.Host,
                                    router,
                                    "POST",
                                    null,
                                    null,
                                    dataBody
                               );
                        var strResponse = api.InteractWithApi();
                        Console.WriteLine("MappingToppingPartner ItemName: {0}", item.ItemNo + "-" + item.RefId.ToString());

                        var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                        if (result.meta.code == 200)
                        {
                            item.Description = result.meta.message ?? "Success";
                            item.IsSync = true;
                            FileHelper.Write2Logs(job_name, "===> MappingToppingPartner result: " + JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            var mess = result.meta.message;
                            var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                            item.Description = resultShopee.result ?? "Failed";
                            //if (resultShopee.result == "error_partner_id_existed")
                            //{
                            //    item.IsSync = true;
                            //}
                        }
                        _mappingRepository.UpdateStatusMapping(job_name, item);
                    }
                    Thread.Sleep(100);
                    ToppingService toppingService = new ToppingService(_interfaceEntry);
                    var lstStore = dataMapping
                              .Select(x => new
                              {
                                  x.StoreNo
                              })
                              .GroupBy(x => new { x.StoreNo })
                              .Select(x =>
                              {
                                  var temp = x.OrderByDescending(o => o.StoreNo).FirstOrDefault();
                                  return new
                                  {

                                      x.Key.StoreNo
                                  };
                              }).ToList();
                    foreach (var store in lstStore)
                    {
                        toppingService.GetToppingByStore(job_name, webApiInfo, store.StoreNo);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> MappingToppingPartner Exception: " + ex.Message.ToString());
            }
        }
        public void MappingToppingGroupPartner(string job_name, WebApiInfo webApiInfo, List<ListMappingDataPartner> data)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                //get data
                var dataMapping = data.Where(x => x.Action.ToUpper() == "MAPPING_TOPPING_GR").ToList();
                if (dataMapping == null)
                {
                    return;
                }
                else if (dataMapping.Count > 0)
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "MappingToppingGroupPartner").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    foreach (var item in dataMapping)
                    {
                        List<topping_groups_mapping> topping_group = new List<topping_groups_mapping>
                        {
                            new topping_groups_mapping()
                            {
                                topping_group_id = item.RefId,
                                partner_topping_group_id = item.ItemNo
                            }
                        };
                        var dataBody = new ToppingGroupMappingRequest()
                        {
                            restaurant_id = item.StoreId,
                            topping_groups = topping_group
                        };

                        FileHelper.Write2Logs(job_name, "===> MappingToppingGroupPartner request: " + JsonConvert.SerializeObject(dataBody));
                        var api = new RestApiHelper(
                                    job_name,
                                    webApiInfo.Host,
                                    router,
                                    "POST",
                                    null,
                                    null,
                                    dataBody
                               );
                        var strResponse = api.InteractWithApi();
                        Console.WriteLine("MappingToppingGroupPartner ItemName: {0}", item.ItemNo + "-" + item.RefId.ToString());

                        var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);
                        if (result.meta.code == 200)
                        {
                            item.Description = result.meta.message ?? "Success";
                            item.IsSync = true;
                            FileHelper.Write2Logs(job_name, "===> MappingToppingGroupPartner result: " + JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            var mess = result.meta.message;
                            var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                            item.Description = resultShopee.result ?? "Failed";
                        }
                        _mappingRepository.UpdateStatusMapping(job_name, item);
                    }

                    Thread.Sleep(100);
                    ToppingService toppingService = new ToppingService(_interfaceEntry);
                    var lstStore = dataMapping
                                  .Select(x => new
                                  {
                                      x.StoreNo
                                  })
                                  .GroupBy(x => new { x.StoreNo })
                                  .Select(x =>
                                  {
                                      var temp = x.OrderByDescending(o => o.StoreNo).FirstOrDefault();
                                      return new
                                      {

                                          x.Key.StoreNo
                                      };
                                  }).ToList();
                    foreach (var store in lstStore)
                    {
                        toppingService.GetShopeeToppingGroupByStore(job_name, webApiInfo, store.StoreNo);
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> MappingToppingGroupPartner Exception: " + ex.Message.ToString());
            }
        }
        public void MappingBlockShopeeDish(string job_name, WebApiInfo webApiInfo, List<ListMappingDataPartner> data)
        {
            try
            {
                if (webApiInfo == null)
                {
                    return;
                }
                var dataMapping = data.Where(x => x.Action == "MAPPING_DISH_LOCK").ToList();
                if(dataMapping.Count > 0)
                {
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "Set_StatusesShopeeDish").FirstOrDefault();
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    foreach(var item in dataMapping)
                    {
                        List<ArrDishSetStatuses> lstDish = new List<ArrDishSetStatuses>
                            {
                                new ArrDishSetStatuses()
                                {
                                    partner_dish_id = item.ItemNo,
                                    status = 3
                                }
                            };
                        var bodyData = new DishSetStatusesRequest()
                        {
                            restaurant_id = item.StoreId,
                            partner_restaurant_id = item.StoreNo,
                            is_apply_all = false,
                            dishes = lstDish
                        };

                        FileHelper.Write2Logs(job_name, "===> MappingBlockShopeeDish request: " + JsonConvert.SerializeObject(bodyData));
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
                        Console.WriteLine("MappingBlockShopeeDish: {0}", item.ItemNo);
                        var result = JsonConvert.DeserializeObject<api_response_meta>(strResponse);

                        if (result.meta.code == 200)
                        {
                            item.Description = result.meta.message ?? "Success";
                            item.IsSync = true;
                            FileHelper.Write2Logs(job_name, "===> MappingBlockShopeeDish result: " + JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            var mess = result.meta.message;
                            var resultShopee = JsonConvert.DeserializeObject<ResultShopeeRsp>(mess);
                            item.Description = resultShopee.result ?? "Failed";
                        }

                        _mappingRepository.UpdateStatusMapping(job_name, item);
                        Thread.Sleep(100);
                        ToppingService toppingService = new ToppingService(_interfaceEntry);
                        var lstStore = dataMapping
                                  .Select(x => new
                                  {
                                      x.StoreNo
                                  })
                                  .GroupBy(x => new { x.StoreNo })
                                  .Select(x =>
                                  {
                                      var temp = x.OrderByDescending(o => o.StoreNo).FirstOrDefault();
                                      return new
                                      {

                                          x.Key.StoreNo
                                      };
                                  }).ToList();
                        foreach (var store in lstStore)
                        {
                            toppingService.GetShopeeToppingGroupByStore(job_name, webApiInfo, store.StoreNo);
                        }

                    }
                }               
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> Set_StatusesShopeeDish Exception: " + ex.Message.ToString());
            }
        }

    }
}
