using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Shared.Enums;
using WebApi.PriceEngine.Application.Interfaces;
using WebApi.PriceEngine.Enums;
using WebApiPriceEngine.Authentication;

namespace WebApi.PriceEngine.Controllers
{
    public class MasterDataController: BaseController
    {
        private readonly ILogger<PriceEngineController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMasterDataService _masterDataService;
        
        private string _proxyHttp = "";
        private string[] _bypassList;

        
        public MasterDataController
        (
             IConfiguration configuration,
             ILogger<PriceEngineController> logger,
             IMasterDataService masterDataService
            
         )
        {
            
            _configuration = configuration;
            _logger = logger;
            if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
            {
                _proxyHttp = _configuration["WebProxy:Http"];
                _bypassList = new string[] { _configuration["WebProxy:Bypasslist"] };
            }
            _masterDataService = masterDataService;
        }
        [HttpGet, Route("api/v1/master-data/table")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public ResponseClient GetTableName()
        {
            try
            {
                return _masterDataService.GetDataTableMaster();
            }
            catch (Exception ex)
            {
                return ResponseHelper.RspNotWarning(9998, ex.Message.ToString());
            }
        }

        [HttpGet, Route("api/v1/master-data/{appCode}/{storeNo}/item/featured")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> GetFeaturedItemAsync([Required] string appCode = "PLH", [Required] string storeNo = "2001")
        {
            try
            {
                return await _masterDataService.GetFeaturedItemMasterAsync(appCode, storeNo, false);
            }
            catch (Exception ex)
            {
                return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, ExceptionHelper.ExptionMessage(ex));
            }
        }

        [HttpGet, Route("api/v1/master-data/{appCode}/{storeNo}/item/promotion")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> GetPromotionItemAsync([Required] string appCode = "PLH", [Required] string storeNo = "2001")
        {
            try
            {
                return await _masterDataService.GetFeaturedItemMasterAsync(appCode, storeNo, true);
            }
            catch (Exception ex)
            {
                return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, ExceptionHelper.ExptionMessage(ex));
            }
        }

        [HttpGet, Route("api/v1/master-data/{appCode}/{tableName}/{maxCounter}/zip", Name = "GetZipFile")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ActionResult> GetZipFileAsync([Required] string appCode = "PLH", [Required] string tableName = "item", [Required] int maxCounter = 0)
        {
            const string contentType = "application/zip";
            HttpContext.Response.ContentType = contentType;
            string projectRootPath = AppContext.BaseDirectory + @"\wwwroot\master_data\";
            try
            {
                return await _masterDataService.GetFileMasterDataAsync(appCode, tableName, maxCounter, projectRootPath, contentType);
            }
            catch 
            {
                return BadRequest();
            }
        }

        [HttpGet, Route("api/v1/master-data/{appCode}/{storeNo}/combo")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> GetComboItemAsync([Required] string appCode = "PLF", [Required] string storeNo = "2001")
        {
            try
            {
                return await _masterDataService.GetComboItemMasterAsync(appCode, storeNo);
            }
            catch (Exception ex)
            {
                return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, ExceptionHelper.ExptionMessage(ex));
            }
        }
    }
}
