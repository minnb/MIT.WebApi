using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.Partner;

namespace VCM.Partner.API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ConfigController : BaseController
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly IPhucLongService _phucLongService;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly ITransService _transService;
        public ConfigController
            (
                ILogger<TransactionController> logger,
                IPhucLongService phucLongService,
                IMemoryCacheService memoryCacheService,
                ITransService transService
            )
        {
            _logger = logger;
            _phucLongService = phucLongService;
            _memoryCacheService = memoryCacheService;
            _transService = transService;
        }

        [HttpGet]
        [Route("/api/v1/config/phuclong/store-kios/list")]
        public async Task<ResponseClient> GetQuestionsPagingAsync([FromQuery] GetStoreKiosPaging query)
        {
            return await _transService.GetStoreAndKiosAsync(query);
        }

        [HttpPost]
        [Route("/api/v1/config/phuclong/store-kios")]
        public async Task<ResponseClient> MappingStoreAndKios([FromHeader] [Required] bool IsMapping = false, [FromHeader] string PartnerCode = "PLG", [FromHeader] string StoreNo = "3737", [FromHeader] string Kios = "KVHCMD3901")
        {
            try
            {
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == PartnerCode.ToUpper()).SingleOrDefault();
                return await _phucLongService.MappingStoreAndKios(_webApiAirPayInfo, IsMapping, StoreNo, Kios);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception MappingStoreAndKios: " + Kios + "|" + StoreNo + " ===>" + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }

        [HttpPost]
        [Route("/api/v1/config/cache/delete")]
        public async Task<ResponseClient> DeleteRedisCacheAsync([FromHeader] [Required] string KeyRedis)
        {
            await _memoryCacheService.RemoveRedisValueAsync(KeyRedis);
            return ResponseHelper.RspOK(ResponseHelper.MetaOK(200, "OK"));
        }

    }
}
