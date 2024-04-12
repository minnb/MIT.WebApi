using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using VCM.Common.Database;
using VCM.Partner.API.Application.Implementation;
using VCM.Partner.API.Application.Interfaces;
using VCM.Shared.Dtos.HCM;
using VCM.Shared.Dtos.WinMoney;

namespace WebApi.Partner.Application.Implementation
{
    public interface IHCMService
    {
        Tuple<HttpStatusCode, string, EmployeeInfoDto> GetEmployeeInfo(string userName);
    }
    public class HCMService : IHCMService
    {
        private string _appHCM = "HCM";
        private readonly ILogger<HCMService> _logger;
        private readonly IMemoryCacheService _memoryCacheService;
        public HCMService
            (
            ILogger<HCMService> logger,
            IMemoryCacheService memoryCacheService
            )
        {
            _logger = logger;
            _memoryCacheService = memoryCacheService;
        }
    
        public Tuple<HttpStatusCode, string, EmployeeInfoDto> GetEmployeeInfo(string userName)
        {
            try
            {
                var webApiConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == _appHCM).FirstOrDefault();
                if(webApiConfig == null)
                {
                    return new Tuple<HttpStatusCode, string, EmployeeInfoDto>(HttpStatusCode.BadGateway, @"WebApi chưa cấu hình kết nối SAP HCM", null);
                }

                DapperContext dapperDbContext = new DapperContext(webApiConfig.Prefix);
                using var conn = dapperDbContext.CreateConnDB;
                conn.Open();
                string query = string.Format(@"EXEC {0} @UserName", webApiConfig.Description);
                var data = conn.Query<EmployeeInfoDto>(query, new { UserName = userName }).FirstOrDefault();
                
                return new Tuple<HttpStatusCode, string, EmployeeInfoDto>(HttpStatusCode.OK, @"OK", data);
            }
            catch(Exception ex) 
            {
                return new Tuple<HttpStatusCode, string, EmployeeInfoDto>(HttpStatusCode.Conflict, JsonConvert.SerializeObject(ex), null);
            }
        }
    }
}
