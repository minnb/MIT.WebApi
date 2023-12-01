using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VCM.Common.Helpers;
using VCM.Shared.Entity.SalesPartner;

namespace Tools.Interface.Services.Shopee
{
    public class MappingRepository : NowRepository
    {
        public MappingRepository(string connectString) : base(connectString)
        {
        }

        public List<ListMappingDataPartner> GetMappingDataPartner(string job_name)
        {
            List<ListMappingDataPartner> result = new List<ListMappingDataPartner>();

            //run store
            var dataMapping = _NowDbContext.MappingDataPartner.FromSqlRaw("EXEC SP_RUN_CREATE_MAPPING_DATA").ToList();
            FileHelper.Write2Logs(job_name, "===> EXEC SP_RUN_CREATE_MAPPING_DATA = " + dataMapping.Count.ToString());
           
            var listDataMapping = _NowDbContext.MappingDataPartner.Where(x => x.IsSync == false).ToList();
            if(listDataMapping.Count > 0)
            {
                var listStore = GetShopeeRestaurant(job_name);
                foreach (var item in listDataMapping)
                {
                    var restaurant = listStore.Where(x => x.partner_restaurant_id == item.StoreNo).FirstOrDefault();
                    if (restaurant != null)
                    {
                        result.Add(new ListMappingDataPartner()
                        {
                            StoreId = restaurant.restaurant_id,
                            AppCode = item.AppCode,
                            StoreNo = item.StoreNo,
                            ItemNo = item.ItemNo,
                            Uom = item.Uom,
                            IsSync = item.IsSync,
                            PictureId = item.PictureId,
                            RefId = item.RefId,
                            Id = item.Id,
                            CrtDate = item.CrtDate,
                            ChgeDate = item.ChgeDate,
                            Action = item.Action
                        });
                    }                   
                }
            }
            return result;
        }
        public bool UpdateStatusMapping(string job_name, MappingDataPartner item)
        {
            try
            {
                var dataUpdate = _NowDbContext.MappingDataPartner.Where(x => x.Id == item.Id).FirstOrDefault();
                if (dataUpdate != null)
                {
                    dataUpdate.ChgeDate = DateTime.Now;
                    dataUpdate.IsSync = item.IsSync;
                    dataUpdate.Description = item.Description;
                    _NowDbContext.Update(dataUpdate);
                    _NowDbContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateStatusMapping Exception: " + ex.Message.ToString());
                return false;
            }
        }
    }
}
