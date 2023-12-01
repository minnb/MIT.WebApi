using System;
using System.Collections.Generic;
using System.Text;
using Tools.Interface.Models;
using VCM.Shared.API.Shopee.Dish;
using VCM.Shared.API.Shopee.Topping;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.SalesPartner;

namespace Tools.Interface.Helpers
{
    public static class ShopeeMappingHelper
    {
        public static WebApiInfo MappingWebApiInfo(SysWebApi header, List<SysWebRoute> detail)
        {
            return new WebApiInfo()
            {
                AppCode = header.AppCode,
                Host = header.Host,
                Description = header.Description,
                UserName = header.UserName,
                Password = header.Password,
                PublicKey = header.PublicKey,
                PrivateKey = header.PrivateKey,
                Blocked = header.Blocked,
                HttpProxy = header.HttpProxy,
                Bypasslist = header.Bypasslist,
                WebRoute = detail
            };
        }
        public static List<ListItemSalesOnApp> MappingListItemSalesOnApp(List<ItemSalesOnApp> itemSalesOnApps)
        {
            List<ListItemSalesOnApp> result = new List<ListItemSalesOnApp>();
            foreach (var item in itemSalesOnApps)
            {
                result.Add(new ListItemSalesOnApp()
                {
                    PartnerCode = item.PartnerCode,
                    AppCode = item.AppCode,
                    StoreNo = item.StoreNo,
                    ItemNo = item.ItemNo,
                    ItemName = item.ItemName,
                    Description = item.Description,
                    Uom = item.Uom,
                    UnitPrice = item.UnitPrice,
                    IsSync = item.IsSync,
                    Blocked = item.Blocked,
                    Size = item.Size,
                    ItemGroup = item.ItemGroup,
                    ItemGroupName = item.ItemGroupName,
                    IsApplyAll = item.IsApplyAll,
                    PictureId = item.PictureId,
                    SyncResults = item.SyncResults,
                    CrtDate = item.CrtDate,
                    ChgeDate = DateTime.Now,
                    Id = item.Id,
                    IsSales = item.IsSales,
                    DishId = item.DishId,
                    CupType = item.CupType,
                    Action = item.Action
                });
            }
            return result;
        }
        public static DishCreateRequest MappingDishCreateRequest(ListItemSalesOnApp item)
        {
            return new DishCreateRequest()
            {
                restaurant_id = item.StoreId,
                partner_restaurant_id = item.StoreNo,
                partner_dish_id = item.ItemNo,
                partner_dish_group_id = item.ItemGroup,
                name = item.ItemName,
                price = item.UnitPrice,
                name_en = "",
                description = item.Description,
                display_order = 0,
                picture_id = item.PictureId,
                is_apply_all = item.IsApplyAll
            };
        }
        public static List<ListItemToppingMapping> MappingListItemSalesOnApp(List<ItemToppingMapping> itemSalesOnApps)
        {
            List<ListItemToppingMapping> result = new List<ListItemToppingMapping>();
            foreach (var item in itemSalesOnApps)
            {
                result.Add(new ListItemToppingMapping()
                {
                    PartnerCode = item.PartnerCode,
                    AppCode = item.AppCode,
                    StoreNo = item.StoreNo,
                    ItemNo = item.ItemNo,        
                    ToppingNo = item.ToppingNo,
                    UnitPrice = item.UnitPrice,
                    MinQuantity = item.MinQuantity,
                    MaxQuantity = item.MaxQuantity,
                    IsRequired = item.IsRequired,
                    Blocked = item.Blocked,
                    IsApplyAll = item.IsApplyAll,
                    IsSync = item.IsSync,
                    IsSetQuantity = item.IsSetQuantity,
                    SyncResults = item.SyncResults,
                    CrtDate = item.CrtDate,
                    ChgeDate = DateTime.Now,
                    Id = item.Id, 
                    Action = item.Action
                });
            }
            return result;
        }
        public static DishCreateToppingMappingRequest MappingDishToppingCreateRequest(ListItemToppingMapping item)
        {
            List<DishCreateToppingMapping> toppings = new List<DishCreateToppingMapping>
            {
                new DishCreateToppingMapping()
                {
                    is_required = item.IsRequired,
                    partner_topping_id = item.ToppingNo,
                    price = item.UnitPrice
                }
            };

            return new DishCreateToppingMappingRequest()
            {
                restaurant_id = item.StoreId,
                partner_restaurant_id = item.StoreNo,
                partner_dish_id = item.ItemNo,
                is_apply_all = item.IsApplyAll,
                toppings = toppings
            };
        }

        //Topping
        public static List<ListToppingSalesOnApp> MappingListToppingSalesOnApp(List<ToppingSalesOnApp> items)
        {
            List<ListToppingSalesOnApp> result = new List<ListToppingSalesOnApp>();
            foreach (var item in items)
            {
                result.Add(new ListToppingSalesOnApp()
                {
                    PartnerCode = item.PartnerCode,
                    AppCode = item.AppCode,
                    StoreNo = item.StoreNo,
                    ToppingGroup = item.ToppingGroup,
                    ToppingNo = item.ToppingNo,
                    Name = item.Name,
                    Name_En = item.Name_En,
                    UnitPrice = item.UnitPrice,
                    DisplayOrder = item.DisplayOrder,
                    Blocked = item.Blocked,
                    IsApplyAll = item.IsApplyAll,
                    IsSync = item.IsSync,
                    SyncResults = item.SyncResults,
                    CrtDate = item.CrtDate,
                    ChgeDate = DateTime.Now,
                    Id = item.Id,
                    Action = item.Action
                });
            }
            return result;
        }
        public static ToppingCreateRequest MappingToppingCreateRequest(ListToppingSalesOnApp item)
        {
            return new ToppingCreateRequest()
            {
                restaurant_id = item.StoreId,
                partner_restaurant_id = item.StoreNo,
                partner_topping_id = item.ToppingNo,
                partner_topping_group_id = item.ToppingGroup,
                name = item.Name,
                name_en = "",
                price = item.UnitPrice,
                display_order = 0,
                is_apply_all = item.IsApplyAll
            };
        }
        public static topping_set_group_quantity MappingToppingSetGroupQuantityRequest(ListItemToppingMapping item)
        {
            List<dishes_topping_set_group_quantity> dishes = new List<dishes_topping_set_group_quantity>()
            {
                new dishes_topping_set_group_quantity()
                {
                    partner_dish_id = item.ItemNo,
                    min_quantity = item.MinQuantity,
                    max_quantity = item.MaxQuantity
                }
            };

            return new topping_set_group_quantity()
            {
                restaurant_id = item.StoreId,
                partner_restaurant_id = item.StoreNo,
                is_apply_all = item.IsApplyAll,
                partner_topping_group_id = item.ToppingNo,
                dishes = dishes
            };
        }
    }
}

