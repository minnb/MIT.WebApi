using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.Entity.Partner;
using WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine;

namespace WebApi.PriceEngine.Application.Interfaces
{
    public interface IRedisCacheService
    {
        public Task<string> GetRedisValueAsync(string key);
        public Task SetRedisKeyAsync(string key, string value);
        public Task<List<UserRoles>> GetUserRolesAsync(bool isDelete = false);
        public Task<List<SysConfig>> GetDataSysConfigAsync(bool isDelete = false);
        public Task<List<SysStoreSet>> GetDataSysStoreSetAsync(bool isDelete = false);
        public Task<byte[]> GetDataByteAsync(string key);
    }
}
