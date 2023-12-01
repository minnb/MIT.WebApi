using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools.Interface.Dtos;
using Tools.Interface.Helpers;
using VCM.Common.Helpers;
using VCM.Shared.API.Shopee.Restaurants;
using VCM.Shared.Entity.Central;

namespace Tools.Interface.Services.Shopee
{
    public class NowService
    {
        private readonly InterfaceEntry _interfaceEntry;
        private NowRepository _nowRepository;
        public NowService
            (
                InterfaceEntry interfaceEntry
            )
        {
            _interfaceEntry = interfaceEntry;
            _nowRepository = new NowRepository(_interfaceEntry.Prefix);
        }

        public void get_restaurant_info(string job_name)
        {
            try
            {
                var lstStore = _nowRepository.GetShopeeRestaurant(job_name, true).ToList();
                if(lstStore!= null && lstStore.Count > 0)
                {
                    var webApiInfo = _nowRepository.GetWebApiInfo(job_name);
                    if (webApiInfo == null)
                    {
                        return;
                    }
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "get_restaurant_info").FirstOrDefault();
                    foreach (var store in lstStore)
                    {
                        var router = routerData.Route + "?StoreNo=" + store.partner_restaurant_id;
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
                            var dataSync = JsonConvert.DeserializeObject<List<RestaurantHeader>>(JsonConvert.SerializeObject(result.data));
                            _nowRepository.UpdateRestauranPartner(job_name, store.partner_restaurant_id, dataSync.FirstOrDefault());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> get_restaurant_info Exception: " + ex.Message.ToString());
            }
        }
    }
}
