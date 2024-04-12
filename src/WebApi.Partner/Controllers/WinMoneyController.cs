using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Emit;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Shared.API;
using VCM.Shared.Dtos.WinMoney;
using VCM.Shared.Enums;
using WebApi.Partner.Authentication;

namespace VCM.Partner.API.Controllers
{
    public class WinMoneyController : BaseController
    {
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IWinMoneyService _winMoneyService;
        public WinMoneyController
            (
                IMemoryCacheService memoryCacheService,
                IWinMoneyService winMoneyService
            ) 
        {
            _memoryCacheService = memoryCacheService;
            _winMoneyService = winMoneyService;
        }

        [HttpPost]
        [Route("api/v1/wmc/login")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.POS })]
        public ResponseClient CreateUrlWinMoney([FromBody] POSRequestUrl_WMC request)
        {
            if (!this.ModelState.IsValid)
            {
                return ResponseHelper.RspNotWarning(400, LogInvalidModelState(ModelState));
            }
            var webApiConfig = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "WMC").SingleOrDefault();
            var url_login = webApiConfig.WebRoute.FirstOrDefault(x => x.Name == "Login");
            if (webApiConfig == null || url_login == null)
            {
                return ResponseHelper.RspNotWarning(400, "WinMoney chưa được cấu hình WebApi, vui lòng liên hệ IT WCM");
            }
            var result = _winMoneyService.RspURL_WMC(webApiConfig, request);
            return result;
        }

        [HttpGet]
        [Route("api/v1/wmc/store")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.POS })]
        public ResponseClient GetStoreInfo([Required]  string appCode)
        {
            if (!this.ModelState.IsValid)
            {
                return ResponseHelper.RspNotWarning(400, LogInvalidModelState(ModelState));
            }

            var username = User.Identity.AuthenticationType;

            var webApiConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == appCode.ToUpper()).FirstOrDefault();
            if (webApiConfig == null)
            {
                return ResponseHelper.RspNotWarning(400, "AppCode: " + appCode + " chưa được cấu hình, vui lòng liên hệ IT WCM");
            }

            return ResponseHelper.RspOK(_winMoneyService.GetStoreInfo(webApiConfig.Prefix));
        }

        [HttpGet]
        [Route("api/v1/wmc/cashier")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.POS })]
        public ResponseClient GetCashierInfo([Required] string appCode, [Required]  string storeNo)
        {
            if (!this.ModelState.IsValid)
            {
                return ResponseHelper.RspNotWarning(400, LogInvalidModelState(ModelState));
            }

            var webApiConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == appCode.ToUpper()).FirstOrDefault();
            if (webApiConfig == null)
            {
                return ResponseHelper.RspNotWarning(400, "AppCode: " + appCode + " chưa được cấu hình, vui lòng liên hệ IT WCM");
            }

            return ResponseHelper.RspOK(_winMoneyService.GetCashierInfo(webApiConfig.Prefix, storeNo));
        }

        [HttpGet]
        [Route("api/v1/wmc/pos")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.POS })]
        public ResponseClient GetPosInfo([Required] string appCode, [Required] string storeNo)
        {
            if (!this.ModelState.IsValid)
            {
                return ResponseHelper.RspNotWarning(400, LogInvalidModelState(ModelState));
            }

            var webApiConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == appCode.ToUpper()).FirstOrDefault();
            if (webApiConfig == null)
            {
                return ResponseHelper.RspNotWarning(400, "AppCode: " + appCode + " chưa được cấu hình, vui lòng liên hệ IT WCM");
            }

            return ResponseHelper.RspOK(_winMoneyService.GetPosInfo(webApiConfig.Prefix, storeNo));
        }


    }
}
