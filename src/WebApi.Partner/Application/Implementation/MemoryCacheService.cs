using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.MBC;
using VCM.Shared.API.O2;
using VCM.Shared.API.Shopee.Dish;
using VCM.Shared.Const;
using VCM.Shared.Dtos.PhucLong;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.SalesPartner;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Dapper;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;
using WebApi.Core.AppServices.ShopeeService;

namespace VCM.Partner.API.Application.Implementation
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly PartnerDbContext _dbContext;
        private readonly IDistributedCache _distributeCache;
        public MemoryCacheService(
            ILogger<MemoryCacheService> logger,
            PartnerDbContext dbContext,
            IDistributedCache distributeCache
        )
        {
            
            _logger = logger;
            _dbContext = dbContext;
            _distributeCache = distributeCache;
        }
        private async Task SetCacheRedisAsync(string key_redis, byte[] data)
        {
            var options = new DistributedCacheEntryOptions()
                                   .SetSlidingExpiration(TimeSpan.FromHours(RedisConst.Redis_cache_time));
            await _distributeCache.SetAsync(key_redis, data, options);
        }
        public async Task<UserMBC> MBCTokenAsync(WebApiViewModel webApiInfo, string proxyHttp, string[] byPass, bool isDelete = false)
        {
            proxyHttp = webApiInfo.HttpProxy;
            byPass = new string[] { webApiInfo.Bypasslist };
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_MBC_token);
            }

            var encodeUserMBC = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_MBC_token);
            if (encodeUserMBC != null)
            {
                _logger.LogWarning("===> Get token from redis");
                return JsonConvert.DeserializeObject<UserMBC>(Encoding.UTF8.GetString(encodeUserMBC));
            }
            else
            {
                _logger.LogWarning("===> Create token from MBC");
                var userMBC = CreateToken(webApiInfo, proxyHttp, byPass);
                if (userMBC != null && !string.IsNullOrEmpty(userMBC.jwtToken))
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_MBC_token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userMBC)));
                }
                return userMBC;
            }
        }
        private UserMBC CreateToken(WebApiViewModel webApiInfo, string proxyHttp, string[] byPass)
        {
            UserMBC userMBC = new UserMBC();
            try
            {
                var routeApi = webApiInfo.WebRoute.Where(x => x.Name == "Login").FirstOrDefault();
                var url_request = webApiInfo.Host + routeApi.Route.ToString();
                var requestMBC = new LoginMBC()
                {
                    username = webApiInfo.UserName,
                    password = webApiInfo.Password,
                    merchantId = webApiInfo.Description
                };
                _logger.LogWarning("CreateToken MBC: " + JsonConvert.SerializeObject(requestMBC));
                ApiHelper api = new ApiHelper(
                    url_request,
                    "",
                    "",
                    "POST",
                    JsonConvert.SerializeObject(requestMBC),
                    false,
                    proxyHttp,
                    byPass
                    );
                var strResponse = api.InteractWithApiResponse();

                if (strResponse != null && strResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string result = string.Empty;
                    using (Stream stream = strResponse.GetResponseStream())
                    {
                        StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                        result = streamReader.ReadToEnd();
                    }
                    _logger.LogWarning("CreateToken MBC: " + result);
                    var objRsp = JsonConvert.DeserializeObject<RspLoginMBC>(result);
                    if(objRsp != null)
                    {
                        if(objRsp.code == "S401")
                        {
                            userMBC = null;
                        }
                        else
                        {
                            string jsonUser = JsonConvert.SerializeObject(objRsp.wsResponse);

                            var infoUser = JsonConvert.DeserializeObject<RspUserLoginMBC>(jsonUser);

                            userMBC.jwtToken = strResponse.Headers[HeaderNames.Authorization];
                            userMBC.username = infoUser.username;
                            userMBC.accountId = infoUser.accountId;
                        }
                    }
                    else
                    {
                        userMBC = null;
                    }
                }
                else
                {
                    _logger.LogWarning("Error response: " + JsonConvert.SerializeObject(strResponse));
                }
            }
            catch (WebException ex)
            {
                using WebResponse response = ex.Response;
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                using Stream data = response.GetResponseStream();
                using var reader = new StreamReader(data);
                _logger.LogWarning("Exception CreateToken MBC: " + reader.ReadToEnd());
            }
            _logger.LogWarning("IMemoryCacheService USER_MBC: " + JsonConvert.SerializeObject(userMBC));
            return userMBC;
        }
        public async Task<List<WebApiViewModel>> GetDataWebApiAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_sys_webapi);
            }

            var dataCache = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_sys_webapi);
            if (dataCache != null)
            {
                return JsonConvert.DeserializeObject<List<WebApiViewModel>>(Encoding.UTF8.GetString(dataCache));
            }
            else
            {
                List<WebApiViewModel> lstDataApi = new List<WebApiViewModel>();
                var infoApi = _dbContext.SysWebApi.Where(x => x.Blocked == false).ToList();
                var routeApi = _dbContext.SysWebRoute.Where(x=>x.Blocked == false).ToList();
                if (infoApi.Count > 0)
                {
                    List<SysWebRoute> lstNewRoute = new List<SysWebRoute>();
                    foreach (var api in infoApi)
                    {
                        lstDataApi.Add(new WebApiViewModel()
                        {
                            Id = api.Id,
                            Host = api.Host,
                            AppCode = api.AppCode,
                            UserName = api.UserName,
                            Password = api.Password,
                            PublicKey = api.PublicKey,
                            PrivateKey = api.PrivateKey,
                            Blocked = api.Blocked,
                            Description = api.Description,
                            WebRoute = routeApi ? .Where(x=>x.AppCode == api.AppCode).ToList(),
                            HttpProxy = api.HttpProxy,
                            Bypasslist = api.Bypasslist
                        });
                        lstNewRoute.Clear();
                    }
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_sys_webapi, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lstDataApi)));
                    
                    return lstDataApi;
                }
                else
                {
                    return null;
                }
            }
        }
        public async Task<List<Item>> GetItemAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_m_item);
            }

            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_m_item);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<Item>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.Item.Where(x => x.Blocked == false).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_m_item, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<List<StoreAndKios>> GetStoreAndKiosAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_map_store_kios);
            }

            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_map_store_kios);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<StoreAndKios>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.StoreAndKios.Where(x => x.Blocked == false).ToList();

                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_map_store_kios, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task SetRedisKeyAsync(string key, string value)
        {
            var checkKey = await _distributeCache.GetAsync(key);
            if (checkKey == null)
            {
                await SetCacheRedisAsync(key, Encoding.UTF8.GetBytes(value));
            }
        }
        public async Task RemoveRedisValueAsync(string key)
        {
            await _distributeCache.RemoveAsync(key);
        }
        public async Task<string> GetRedisValueAsync(string key)
        {
            var redis_data = await _distributeCache.GetAsync(key);
            if (redis_data != null)
            {
                return Encoding.UTF8.GetString(redis_data);
            }
            else
            {
                return null;
            }
        }
        public async Task<List<SysConfig>> GetDataSysConfigAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_sys_config);
            }
            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_sys_config);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<SysConfig>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.SysConfig.Where(x => x.Blocked == false).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_sys_config, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<List<UserRoles>> GetUserRolesAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_sys_user_roles);
            }
            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_sys_user_roles);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<UserRoles>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.UserRoles.Where(x => x.Blocked == false).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_sys_user_roles, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<List<CpnVchBOMHeaderDto>> GetCpnVchBOMHeaderAsync(bool isDelete, string appCode, string function)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_pos_CpnVchBOMHeader);
            }
           
            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_pos_CpnVchBOMHeader);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<CpnVchBOMHeaderDto>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var sysConfig = GetDataSysConfigAsync().Result?.Where(x => x.AppCode == appCode && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                if (sysConfig != null )
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    string query = @"select distinct ItemNo, ItemName,UnitOfMeasure, DiscountType, cast(DiscountValue as decimal) DiscountValue, cast(MaxAmount as decimal) MaxAmount, cast(ValueOfVoucher as decimal) ValueOfVoucher, ArticleType, StartingDate, EndingDate, Blocked, LimitQty, CpnVchType
                                        from CpnVchBOMHeader (nolock)
                                        where Blocked = 0";
                    var data = conn.Query<CpnVchBOMHeaderDto>(query).ToList();
                    if (data.Count > 0)
                    {
                        await SetCacheRedisAsync(RedisConst.Redis_cache_pos_CpnVchBOMHeader, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }
        public async Task<List<ItemDto>> GetItemPhucLongAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_master_item_phuclong);
            }

            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_master_item_phuclong);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<ItemDto>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                string function = "CentralMD";
                var sysConfig = GetDataSysConfigAsync().Result?.Where(x => x.AppCode == "PLH" && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                if (sysConfig != null)
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    string query = @"EXEC SP_API_GET_ITEMS;";
                    var data = conn.Query<ItemDto>(query).ToList();
                    if (data.Count > 0)
                    {
                        await SetCacheRedisAsync(RedisConst.Redis_master_item_phuclong, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }
        public async Task<List<ShopeeRestaurant>> GetShopeeRestaurantAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_ShopeeRestaurant);
            }
            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_ShopeeRestaurant);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<ShopeeRestaurant>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.ShopeeRestaurant.Where(x=>x.restaurant_id > 0).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_ShopeeRestaurant, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<TokenO2> TokenO2Async(WebApiViewModel webApiInfo, bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_O2_Token);
            }

            var encodeToken = await _distributeCache.GetAsync(RedisConst.Redis_O2_Token);
            if (encodeToken != null)
            {
                _logger.LogWarning("===> Get token from redis");
                return JsonConvert.DeserializeObject<TokenO2>(Encoding.UTF8.GetString(encodeToken));
            }
            else
            {
                _logger.LogWarning("===> Create token from MBC");
                var dataTokenO2 = CreateTokenO2(webApiInfo);
                if (dataTokenO2 != null && !string.IsNullOrEmpty(dataTokenO2.Data.Token))
                {
                    await SetCacheRedisAsync(RedisConst.Redis_O2_Token, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataTokenO2)));
                }

                return dataTokenO2;
            }
        }
        private TokenO2 CreateTokenO2(WebApiViewModel webApiInfo)
        {
            TokenO2 dataToken = new TokenO2();
            try
            {
                var routeApi = webApiInfo.WebRoute.Where(x => x.Name == "Login").FirstOrDefault();
                var url_request = webApiInfo.Host + routeApi.Route.ToString();
                var request = new UserLoginO2()
                {
                    ConsumerKey = webApiInfo.UserName,
                    Password = webApiInfo.Password
                };
                
                string[] byPass = new string[] { webApiInfo.Bypasslist };

                _logger.LogWarning(url_request);
                _logger.LogWarning("Proxy: " + webApiInfo.HttpProxy);
                _logger.LogWarning("byPass: " + webApiInfo.Bypasslist);
                ApiHelper api = new ApiHelper(
                    url_request,
                    "",
                    "",
                    "POST",
                    JsonConvert.SerializeObject(request),
                    false,
                    webApiInfo.HttpProxy,
                    byPass
                    );
                var strResponse = api.InteractWithApiResponse();
                _logger.LogWarning("Login O2 response: " + strResponse);
                if (strResponse != null && strResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogWarning("O2 error response:  " + JsonConvert.SerializeObject(strResponse));
                    string result = string.Empty;
                    using (Stream stream = strResponse.GetResponseStream())
                    {
                        StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                        result = streamReader.ReadToEnd();
                    }
                    _logger.LogWarning("CreateToken O2: " + result);
                    if (!string.IsNullOrEmpty(result))
                    {
                        var objRsp = JsonConvert.DeserializeObject<TokenO2>(result);
                        if(objRsp != null)
                        {
                            return objRsp;
                        }
                        else
                        {
                           return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (WebException ex)
            {
                using WebResponse response = ex.Response;
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                using Stream data = response.GetResponseStream();
                using var reader = new StreamReader(data);
                _logger.LogWarning("Exception CreateToken O2: " + reader.ReadToEnd());
                return null;
            }

        }
        public async Task<List<TenderTypeSetup>> GetTenderTypeSetupAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_m_tender_type_setup);
            }

            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_m_tender_type_setup);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<TenderTypeSetup>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.TenderTypeSetup.Where(x => x.Blocked == false).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_m_tender_type_setup, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<List<StoreMaster>> GetAllStoreAsync(string appCode,bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_store_master);
            }

            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_store_master);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<StoreMaster>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                string function = "CentralMD";
                var sysConfig = GetDataSysConfigAsync().Result?.Where(x => x.AppCode == appCode.ToUpper() && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                if (sysConfig != null)
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    string query = @"EXEC SP_API_GET_STORE;";
                    var data = conn.Query<StoreMaster>(query).ToList();
                    if (data.Count > 0)
                    {
                        await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_store_master, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
