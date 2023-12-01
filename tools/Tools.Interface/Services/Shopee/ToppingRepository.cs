using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tools.Interface.Database;
using Tools.Interface.Helpers;
using VCM.Common.Helpers;
using VCM.Shared.API.Shopee.Topping;
using VCM.Shared.Entity.SalesPartner;

namespace Tools.Interface.Services.Shopee
{
    public class ToppingRepository : NowRepository
    {
        public ToppingRepository(string connectString) : base(connectString)
        {
        }

        //TOPPING
        public bool SyncTopping(string job_name, List<toppings> dataSync, string partner_retaurant_id)
        {
            try
            {
                if (dataSync.Count > 0)
                {
                    var dataRemove = _NowDbContext.ShopeeTopping.Where(x => x.partner_restaurant_id == partner_retaurant_id).ToList();
                    dataRemove.ForEach(x => _NowDbContext.ShopeeTopping.Remove(x));
                    List<ShopeeTopping> dataIns = new List<ShopeeTopping>();
                    foreach (var item in dataSync)
                    {
                        dataIns.Add(new ShopeeTopping()
                        {
                            topping_id = item.topping_id,
                            topping_group_id = item.topping_group_id,
                            partner_topping_group_id = item.partner_topping_group_id,
                            partner_topping_id = item.partner_topping_id,
                            name = item.name,
                            name_en = item.name_en,
                            display_order = item.display_order,
                            price = item.price,
                            is_active = item.is_active,
                            partner_restaurant_id = partner_retaurant_id
                        });
                    }
                    dataIns.ForEach(x => _NowDbContext.ShopeeTopping.Add(x));
                    _NowDbContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> SyncTopping Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public bool SyncToppingGroup(string job_name, List<get_topping_groups> dataSync, string partner_retaurant_id)
        {
            try
            {
                if (dataSync.Count > 0)
                {
                    var dataRemove = _NowDbContext.ShopeeToppingGroup.Where(x => x.partner_restaurant_id == partner_retaurant_id).ToList();
                    dataRemove.ForEach(x => _NowDbContext.ShopeeToppingGroup.Remove(x));
                    List<ShopeeToppingGroup> dataIns = new List<ShopeeToppingGroup>();
                    foreach (var item in dataSync)
                    {
                        dataIns.Add(new ShopeeToppingGroup()
                        {
                            topping_group_id = item.topping_group_id,
                            partner_topping_group_id = item.partner_topping_group_id,
                            name = item.name,
                            name_en = item.name_en,
                            display_order = item.display_order,
                            is_active = item.is_active,
                            description = item.description,
                            partner_restaurant_id = partner_retaurant_id
                        });
                    }
                    dataIns.ForEach(x => _NowDbContext.ShopeeToppingGroup.Add(x));
                    _NowDbContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> SyncToppingGroup Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public List<ListToppingSalesOnApp> GetToppingCreate(string job_name)
        {
            try
            {
                List<ListToppingSalesOnApp> listItemSalesOnApps = new List<ListToppingSalesOnApp>();
                var itemSalesOnApp = _NowDbContext.ToppingSalesOnApp.FromSqlRaw("EXEC PLH_NOW_TOPPING_CREATE").ToList();
                if (itemSalesOnApp.Count > 0)
                {
                    var listStore = GetShopeeRestaurant(job_name);
                    listItemSalesOnApps = ShopeeMappingHelper.MappingListToppingSalesOnApp(itemSalesOnApp);
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
                            UpdateStatusToppingCreate(job_name, item, false);
                            itemSalesOnApp.Remove(item);
                            FileHelper.Write2Logs(job_name, "===> GetToppingCreate remove: " + JsonConvert.SerializeObject(item));
                        }
                    }
                }

                return listItemSalesOnApps;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> GetToppingCreate Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public bool UpdateStatusToppingCreate(string job_name, ListToppingSalesOnApp itemSalesOnApp, bool isAPI = true)
        {
            try
            {
                var updateStatus = _NowDbContext.ToppingSalesOnApp.Where(x => x.Id == itemSalesOnApp.Id).FirstOrDefault();
                if (updateStatus != null && isAPI == true)
                {
                    updateStatus.SyncResults = itemSalesOnApp.SyncResults;
                    updateStatus.IsSync = itemSalesOnApp.IsSync;
                    updateStatus.ChgeDate = DateTime.Now;
                    _NowDbContext.Update(updateStatus);
                }
                else
                {
                    updateStatus.SyncResults = itemSalesOnApp.SyncResults;
                    updateStatus.ChgeDate = DateTime.Now;
                    _NowDbContext.Update(updateStatus);
                }
                _NowDbContext.SaveChanges();
                Thread.Sleep(10);
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateStatusToppingCreate Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public List<ItemToppingMapping> GetDishPartner_From_ToppingPartner(string toppingNo, string storeNo)
        {
            return _NowDbContext.ItemToppingMapping.Where(x => x.ToppingNo == toppingNo && x.Blocked == false && x.StoreNo == storeNo).ToList();
        }
        public bool UpdateItemToppingMapping(string job_name, List<ItemToppingMapping> item)
        {
            try
            {
                item.ForEach(x => _NowDbContext.Update(x));
                _NowDbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateItemToppingMapping Exception: " + ex.Message.ToString());
                return false;
            }
        }
    }
}
