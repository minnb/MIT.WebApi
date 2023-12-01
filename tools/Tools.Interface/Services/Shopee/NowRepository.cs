using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools.Interface.Database;
using Tools.Interface.Helpers;
using Tools.Interface.Models;
using VCM.Common.Helpers;
using VCM.Shared.API.Shopee.Restaurants;
using VCM.Shared.Entity.SalesPartner;

namespace Tools.Interface.Services.Shopee
{
    public class NowRepository
    {
        public PartnerMDDbContext _NowDbContext;
        public readonly string AppCodeNow = "SHOPEE";
        private readonly string _connectString;
        public string[] lstStautsBlock = new string[] { "BLOCKED", "ACTIVE", "OUT_OFF_STOCK" };
        public string[] lstStautsUpdatePrice = new string[] { "UPDATE_PRICE" };
        public string[] lstStautsUpdateInfo = new string[] { "UPDATE_INFO" };
        public NowRepository
            (
                string connectString
            )
        {
            _connectString = connectString;
            _NowDbContext = new PartnerMDDbContext(_connectString);
        }
        public List<ShopeeRestaurant> GetShopeeRestaurant(string job_name, bool is_restaurant_id = false)
        {
            try
            {
                if(!is_restaurant_id)
                {
                    return _NowDbContext.ShopeeRestaurant.Where(x => x.restaurant_id > 0).ToList();
                }
                else
                {
                    return _NowDbContext.ShopeeRestaurant.Where(x => x.restaurant_id == 0).ToList();
                }
                
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> GetShopeeRestaurant Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public bool UpdateRestauranPartner(string job_name, string partner_restaurant_id, RestaurantHeader item)
        {
            try
            {
                var updateData = _NowDbContext.ShopeeRestaurant.Where(x => x.partner_restaurant_id == partner_restaurant_id).FirstOrDefault();
                if (updateData != null)
                {
                    updateData.restaurant_id = item.restaurant_id;
                    updateData.name = item.name;
                    updateData.address = item.address;
                    updateData.foody_service = item.foody_service;
                    updateData.city = item.city;
                    _NowDbContext.Update(updateData);
                    _NowDbContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateRestauranPartner Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public WebApiInfo GetWebApiInfo(string job_name)
        {
            try
            {
                var header = _NowDbContext.SysWebApi.Where(x => x.AppCode == AppCodeNow && x.Blocked == false).FirstOrDefault();
                if (header != null)
                {
                    var detail = _NowDbContext.SysWebRoute.Where(x => x.AppCode == AppCodeNow && x.Blocked == false).ToList();
                    return ShopeeMappingHelper.MappingWebApiInfo(header, detail);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> GetWebApiInfo Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public bool UpdateStatusActionLogging(string job_name, string id, bool status, string action, bool statusUpdate, string message)
        {
            try
            {
                var updateStatus = _NowDbContext.ActionLogging.Where(x => x.RefId == id && x.Action == action && x.Status == status).FirstOrDefault();
                if (updateStatus != null)
                {
                    updateStatus.Status = statusUpdate;
                    updateStatus.Description = message;
                    updateStatus.ChgeDate = DateTime.Now;
                    _NowDbContext.Update(updateStatus);
                    _NowDbContext.SaveChanges();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateStatusDishCreate Exception: " + ex.Message.ToString());
                return false;
            }
        }

    }
}
