using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Common.Helpers;
using VCM.Partner.API.Database;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.MBC;
using VCM.Shared.Const;
using VCM.Shared.Entity.Partner;

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
                                    //.SetAbsoluteExpiration(DateTime.Now.AddHours(12));
            await _distributeCache.SetAsync(key_redis, data, options);
        }    
        public async Task<UserMBC> MBCTokenAsync(WebApiViewModel webApiInfo, string proxyHttp, string[] byPass, bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_MBC_token);
            }

            var encodeUserMBC = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_MBC_token);
            if (encodeUserMBC != null)
            {
                _logger.LogWarning("Get token from redis");
                return JsonConvert.DeserializeObject<UserMBC>(Encoding.UTF8.GetString(encodeUserMBC));
            }
            else
            {
                var userMBC = CreateToken(webApiInfo, proxyHttp, byPass);
                if (userMBC != null)
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
                    string jsonUser = JsonConvert.SerializeObject(objRsp.wsResponse);

                    var infoUser = JsonConvert.DeserializeObject<RspUserLoginMBC>(jsonUser);

                    userMBC.jwtToken = strResponse.Headers[HeaderNames.Authorization];
                    userMBC.username = infoUser.username;
                    userMBC.accountId = infoUser.accountId;
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
                            WebRoute = routeApi ? .Where(x=>x.AppCode == api.AppCode).ToList()
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
    }
}
