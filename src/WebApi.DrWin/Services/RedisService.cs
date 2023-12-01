using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Shared.Const;
using VCM.Shared.Dtos;
using VCM.Shared.Entity.Partner;
using WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin;
using WebApi.DrWin.Models;

namespace WebApi.DrWin.Services
{
    public interface IRedisService
    {
        public Task<List<SysWebApiDto>> GetDataWebApiAsync(bool isDelete = false);
        public Task<Tai_khoan_ket_noi_drw> GetTokenAsync(SysWebApiDto webApiInfo, string ma_sap);
        public Task<List<Tai_khoan_ket_noi_drw>> GetTaiKhoanKetNoiCD();
    }
    public class RedisService: IRedisService
    {
        private readonly ILogger<RedisService> _logger;
        private readonly DrWinDbContext _dbContext;
        private readonly IDistributedCache _distributeCache;
        private readonly IApiService _apiService;
        public RedisService(
            ILogger<RedisService> logger,
            DrWinDbContext dbContext,
            IDistributedCache distributeCache,
            IApiService apiService
        )
        {

            _logger = logger;
            _dbContext = dbContext;
            _distributeCache = distributeCache;
            _apiService = apiService;
        }
        private async Task SetCacheRedisAsync(string key_redis, byte[] data)
        {
            var options = new DistributedCacheEntryOptions()
                                   .SetSlidingExpiration(TimeSpan.FromHours(RedisConst.Redis_cache_time));
            await _distributeCache.SetAsync(key_redis, data, options);
        }
        public async Task<List<SysWebApiDto>> GetDataWebApiAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_sys_webapi);
            }

            var dataCache = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_sys_webapi);
            if (dataCache != null)
            {
                return JsonConvert.DeserializeObject<List<SysWebApiDto>>(Encoding.UTF8.GetString(dataCache));
            }
            else
            {
                List<SysWebApiDto> lstDataApi = new List<SysWebApiDto>();
                var infoApi = _dbContext.SysWebApi.Where(x => x.Blocked == false).ToList();
                var routeApi = _dbContext.SysWebRoute.Where(x => x.Blocked == false).ToList();
                if (infoApi.Count > 0)
                {
                    List<SysWebRoute> lstNewRoute = new List<SysWebRoute>();
                    foreach (var api in infoApi)
                    {
                        lstDataApi.Add(new SysWebApiDto()
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
                            WebRoute = routeApi?.Where(x => x.AppCode == api.AppCode).ToList(),
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
        public async Task<Tai_khoan_ket_noi_drw> GetTokenAsync(SysWebApiDto webApiInfo, string ma_sap)
        {
            string function = "Login";
            string key = RedisConst.Redis_Drw_Token + "_" + ma_sap;
            Tai_khoan_ket_noi_drw tokenDrw = new Tai_khoan_ket_noi_drw();

            var dataCache = await _distributeCache.GetAsync(key);
            if (dataCache != null)
            {
                tokenDrw = JsonConvert.DeserializeObject<Tai_khoan_ket_noi_drw>(Encoding.UTF8.GetString(dataCache));
            }
            else
            {
                try
                {
                    if (ma_sap != null)
                    {
                        var tai_khoan_ket_noi = _dbContext.Tai_Khoan_Ket_Noi.Where(x => x.Ma_sap == ma_sap).FirstOrDefault();
                        if (tai_khoan_ket_noi != null)
                        {
                            var user = new LoginDrwModel()
                            {
                                Usr = tai_khoan_ket_noi.Tai_khoan_ket_noi,
                                Pwd = tai_khoan_ket_noi.Mat_khau
                            };
                            var result = await _apiService.CallApiHttpWeb(webApiInfo, JsonConvert.SerializeObject(user), null, function, null, "POST", ma_sap);
                            if (!string.IsNullOrEmpty(result))
                            {
                                var loginRsp = JsonConvert.DeserializeObject<LoginRsp>(result.ToString());
                                if(loginRsp != null)
                                {
                                    tokenDrw.Ma_sap = ma_sap;
                                    tokenDrw.Ma_co_so = tai_khoan_ket_noi.Ma_co_so;
                                    tokenDrw.Tai_khoan_ket_noi = tai_khoan_ket_noi.Tai_khoan_ket_noi;
                                    tokenDrw.Token = loginRsp.Data.Token;
                                    tokenDrw.Token_type = loginRsp.Data.Token_type;
                                    await SetCacheRedisAsync(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tokenDrw)));
                                }
                                else
                                {
                                    tokenDrw = null;
                                }                               
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("GetTokenAsync.Exception: " + ex.Message.ToString());
                    tokenDrw = null;
                }
            }
            return tokenDrw;
        }

        public async Task<List<Tai_khoan_ket_noi_drw>> GetTaiKhoanKetNoiCD()
        {
            var dataCache = await _distributeCache.GetAsync(RedisConst.Redis_Drw_Tai_khoan_ket_noi);
            if (dataCache != null)
            {
                return JsonConvert.DeserializeObject<List<Tai_khoan_ket_noi_drw>>(Encoding.UTF8.GetString(dataCache));
            }
            else
            {
                var dataTK = _dbContext.Tai_Khoan_Ket_Noi.ToList();
                if(dataTK.Count > 0)
                {
                    List<Tai_khoan_ket_noi_drw> result = new List<Tai_khoan_ket_noi_drw>();
                    foreach(var item in dataTK)
                    {
                        result.Add(new Tai_khoan_ket_noi_drw()
                        {
                            Ma_sap = item.Ma_sap,
                            Ma_co_so = item.Ma_co_so,
                            Tai_khoan_ket_noi = item.Tai_khoan_ket_noi
                        });
                    }
                    await SetCacheRedisAsync(RedisConst.Redis_Drw_Tai_khoan_ket_noi, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)));
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
