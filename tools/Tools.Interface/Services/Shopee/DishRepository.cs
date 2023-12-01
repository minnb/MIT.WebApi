using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tools.Interface.Helpers;
using VCM.Common.Helpers;
using VCM.Shared.API.Shopee.Dish;
using VCM.Shared.Entity.SalesPartner;

namespace Tools.Interface.Services.Shopee
{
    public class DishRepository: NowRepository
    {
        public DishRepository(string connectString) : base(connectString)
        {
        }

        //DISH GROUP
        public bool SyncDishGroup(string job_name, List<DishGroupShopee> dishGroupShopees, string partner_retaurant_id)
        {
            try
            {
                if(dishGroupShopees.Count > 0)
                {
                    var dataRemove = _NowDbContext.ShopeeDishGroup.Where(x => x.partner_restaurant_id == partner_retaurant_id).ToList();
                    dataRemove.ForEach(x => _NowDbContext.ShopeeDishGroup.Remove(x));
                    List<ShopeeDishGroup> shopeeDishGroups = new List<ShopeeDishGroup>();
                    foreach(var item in dishGroupShopees)
                    {
                        shopeeDishGroups.Add(new ShopeeDishGroup()
                        {
                            dish_group_id = item.dish_group_id,
                            partner_dish_group_id = item.partner_dish_group_id,
                            display_order = item.display_order,
                            name = item.name,
                            name_en  = item.name_en,
                            description = item.description,
                            crt_date = DateTime.Now,
                            partner_restaurant_id = partner_retaurant_id
                        });
                    }
                    shopeeDishGroups.ForEach(x => _NowDbContext.ShopeeDishGroup.Add(x));
                    _NowDbContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> SyncDishGroup Exception: " + ex.Message.ToString());
                return false;
            }
        }
        //DISH 
        public bool SyncDish(string job_name, List<DishesShopee> dishesShopee, string partner_retaurant_id)
        {
            try
            {
                if (dishesShopee.Count > 0)
                {
                    var dataRemove = _NowDbContext.ShopeeDish.Where(x => x.partner_restaurant_id == partner_retaurant_id).ToList();
                    dataRemove.ForEach(x => _NowDbContext.ShopeeDish.Remove(x));
                    List<ShopeeDish> dataIns = new List<ShopeeDish>();
                    foreach (var item in dishesShopee)
                    {
                        var pic_id = 0;
                        var url_pic = "";
                        if (item.picture != null)
                        {
                            pic_id = item.picture.id;
                            url_pic = item.picture.url;
                        }
                        dataIns.Add(new ShopeeDish()
                        {
                            dish_id = item.dish_id,
                            dish_group_id = item.dish_group_id,
                            partner_dish_group_id = item.partner_dish_group_id??"",
                            display_order = item.display_order,
                            name = item.name ?? "",
                            name_en = item.name_en ?? "",
                            price = item.price,
                            is_active = item.is_active,
                            description = item.description ?? "",
                            created_time = item.created_time,
                            updated_time = item.updated_time?? item.created_time,
                            partner_restaurant_id = partner_retaurant_id,
                            picture_id = pic_id,
                            url = url_pic,
                            partner_dish_id = item.partner_dish_id ?? ""
                        });
                    }
                    dataIns.ForEach(x => _NowDbContext.ShopeeDish.Add(x));
                    _NowDbContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> SyncDish Exception: " + ex.Message.ToString());
                return false;
            }
        }
        //GET DISH CREATE
        public List<ListItemSalesOnApp> GetDishCreate(string job_name)
        {
            try
            {
                List<ListItemSalesOnApp> listItemSalesOnApps = new List<ListItemSalesOnApp>();
                var listStore = GetShopeeRestaurant(job_name);
                var itemSalesOnApp = _NowDbContext.ItemSalesOnApp.FromSqlRaw("EXEC PLH_NOW_DISH_CREATE").ToList();
                if(itemSalesOnApp.Count > 0)
                {
                    listItemSalesOnApps = ShopeeMappingHelper.MappingListItemSalesOnApp(itemSalesOnApp);
                    foreach (var item in listItemSalesOnApps)
                    {
                        var restaurant = listStore.Where(x => x.partner_restaurant_id == item.StoreNo).FirstOrDefault();
                        
                        if (restaurant != null)
                        {
                            item.StoreId = restaurant.restaurant_id;
                            item.StoreNo = restaurant.partner_restaurant_id;
                        }
                        else
                        {
                            item.SyncResults = "NOT_FOUND_STORE_ON_NOW";
                            UpdateStatusDishCreate(job_name, item, false);
                            itemSalesOnApp.Remove(item);
                            FileHelper.Write2Logs(job_name, "===> GetDishCreate remove: " + JsonConvert.SerializeObject(item));
                        }
                    }
                }

                return listItemSalesOnApps;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> GetDishCreate Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public bool UpdateStatusDishCreate(string job_name, ListItemSalesOnApp item, bool isAPI = true)
        {
            try
            {
                var updateStatus = _NowDbContext.ItemSalesOnApp.Where(x => x.Id == item.Id).FirstOrDefault();
                if (updateStatus != null)
                {
                    if (isAPI == true)
                    {
                        updateStatus.SyncResults = item.SyncResults;
                        updateStatus.IsSync = item.IsSync;
                        updateStatus.ChgeDate = DateTime.Now;
                        _NowDbContext.Update(updateStatus);
                    }
                    else
                    {
                        updateStatus.SyncResults = item.SyncResults;
                        updateStatus.ChgeDate = DateTime.Now;
                        _NowDbContext.Update(updateStatus);
                    }
                }
                _NowDbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateStatusDishCreate Exception: " + ex.Message.ToString());
                return false;
            }
        }

        public List<ListItemToppingMapping> DishToppingMappingCreate(string job_name, bool isSetQuantity = false)
        {
            try
            {
                List<ListItemToppingMapping> listItemToppingMapping = new List<ListItemToppingMapping>();
                var listStore = GetShopeeRestaurant(job_name);
                var itemSalesOnApp = new List<ItemToppingMapping>();
                if (isSetQuantity)
                {
                    itemSalesOnApp = _NowDbContext.ItemToppingMapping.FromSqlRaw("EXEC PLH_NOW_TOPPING_SET_GROUP_QUANTITY").ToList();
                }
                else
                {
                    itemSalesOnApp = _NowDbContext.ItemToppingMapping.FromSqlRaw("EXEC PLH_NOW_DISH_CREATE_TOPPING_MAPPING").ToList();
                }
                
                if (itemSalesOnApp.Count > 0)
                {
                    listItemToppingMapping = ShopeeMappingHelper.MappingListItemSalesOnApp(itemSalesOnApp);
                    foreach (var item in listItemToppingMapping)
                    {
                        var restaurant = listStore.Where(x => x.partner_restaurant_id == item.StoreNo).FirstOrDefault();
                        if (restaurant != null)
                        {
                            item.StoreId = restaurant.restaurant_id;
                            item.StoreNo = restaurant.partner_restaurant_id;
                        }
                        else
                        {
                            itemSalesOnApp.Remove(item);
                            FileHelper.Write2Logs(job_name, "===> DishToppingMappingCreate remove: " + JsonConvert.SerializeObject(item));
                        }
                    }
                }

                return listItemToppingMapping;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> DishToppingMappingCreate Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public bool UpdateStatusDishToppingMappingCreate(string job_name, ListItemToppingMapping dataUpdate, bool isAPI = true)
        {
            try
            {
                var updateStatus = _NowDbContext.ItemToppingMapping.Where(x => x.Id == dataUpdate.Id).FirstOrDefault();
                if (updateStatus != null && isAPI == true)
                {
                    updateStatus.SyncResults = dataUpdate.SyncResults;
                    updateStatus.IsSync = dataUpdate.IsSync;
                    updateStatus.ChgeDate = DateTime.Now;
                    _NowDbContext.Update(updateStatus);
                }
                else
                {
                    updateStatus.SyncResults = dataUpdate.SyncResults;
                    updateStatus.ChgeDate = DateTime.Now;
                    _NowDbContext.Update(updateStatus);
                }

                _NowDbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateStatusDishToppingMappingCreate Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public bool UpdateStatusSetQuantityTopping(string job_name, ListItemToppingMapping itemSalesOnApp, bool isAPI = true)
        {
            try
            {
                var updateStatus = _NowDbContext.ItemToppingMapping.Where(x => x.Id == itemSalesOnApp.Id).FirstOrDefault();
                if (updateStatus != null && isAPI == true)
                {
                    updateStatus.SyncSetQuantity = itemSalesOnApp.SyncSetQuantity;
                    updateStatus.IsSetQuantity = itemSalesOnApp.IsSetQuantity;
                    updateStatus.ChgeDate = DateTime.Now;
                    _NowDbContext.Update(updateStatus);
                }
                else
                {
                    updateStatus.SyncSetQuantity = itemSalesOnApp.SyncSetQuantity;
                    updateStatus.ChgeDate = DateTime.Now;
                    _NowDbContext.Update(updateStatus);
                }
                _NowDbContext.SaveChanges();
                Thread.Sleep(10);
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateStatusSetQuantityTopping Exception: " + ex.Message.ToString());
                return false;
            }
        }
    }
}
