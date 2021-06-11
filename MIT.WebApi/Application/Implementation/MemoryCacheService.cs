using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MIT.Dtos;
using MIT.EntityFrameworkCore;
using MIT.WebApi.GPAY.Application.Interfaces;
using MIT.WebApi.GPAY.ViewModels.AirPay;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MIT.WebApi.GPAY.Application.Implementation
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly string _memory_cache_web_api = "YUhSMGNITTZMeTlrY0M1MVlYUXVaR0ZwYkhsemFHOXdaV1V1ZG00dlkyaGhhVzVmYzNSdmNtVXZZV2x5Y0dGNQ==";
        private IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }
        public List<WebApiViewModel> GetDataWebApi()
        {
            if (!_memoryCache.TryGetValue(_memory_cache_web_api, out List<WebApiViewModel> _webApiAir))
            {
                List<WebApiViewModel> lstDataApi = new List<WebApiViewModel>();
                using (var dbcontext = new GPAYDbContext())
                {
                    try
                    {
                        var infoApi = dbcontext.WebApiDto.Where(x => x.Blocked == false).ToList();
                        if (infoApi != null)
                        {
                            List<WebRouteDto> lstNewRoute = new List<WebRouteDto>();
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
                                    WebRouteDto = dbcontext.WebRouteDto.Where(x => x.AppCode == api.AppCode).ToList()
                                });
                                lstNewRoute.Clear();
                            }

                            _webApiAir = lstDataApi;
                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                                .SetSize(1)
                                                .SetSlidingExpiration(TimeSpan.FromHours(24));
                            _memoryCache.Set(_memory_cache_web_api, _webApiAir, cacheEntryOptions);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("IMemoryCacheService.GetDataWebApi Exception" + ex.Message.ToString());
                    }
                }
            }
            return _webApiAir;
        }
    }
}
