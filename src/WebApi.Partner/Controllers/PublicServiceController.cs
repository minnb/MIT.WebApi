using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Controllers;
using VCM.Shared.API;
using VCM.Shared.API.O2;
using VCM.Shared.API.PublishService;
using VCM.Shared.Enums;
using WebApi.Partner.Application.Interfaces;
using WebApi.Partner.Authentication;

namespace WebApi.Partner.Controllers
{
    public class PublicServiceController : BaseController
    {
        private readonly ILogger<PartnerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheService _memoryCacheService;
        public PublicServiceController
            (
                 IConfiguration configuration,
                 ILogger<PartnerController> logger,
                 IMemoryCacheService memoryCacheService
            )
        {
            _configuration = configuration;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
        }

        [HttpGet]
        [Route("api/v1/sevices/tax-info/{taxcode}")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public ResponseClient GetTaxtInfo([Required] string taxcode = "0302616081-001")
        {
            try
            {
                var webApiInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode.ToUpper() == "VIETQR").FirstOrDefault();
                if (webApiInfo == null)
                {
                    return ResponseHelper.RspNotWarning(9998, "Chưa khai báo VIETQR = api.vietqr.io");
                }

                var routeApi = webApiInfo.WebRoute.Where(x => x.Name == "business").FirstOrDefault();
                var url_request = webApiInfo.Host + routeApi.Route.ToString() + @"/" + taxcode;

                ApiHelper api = new ApiHelper(
                    url_request,
                    null,
                    null,
                    "GET",
                    url_request,
                    false,
                    webApiInfo.HttpProxy,
                    new string[] { webApiInfo.Bypasslist }
                    );
                string result = api.InteractWithApi();
                if(!string.IsNullOrEmpty( result ) )
                {
                    var objRsp = JsonConvert.DeserializeObject<ResponseTaxVietQR>(result);
                    if (objRsp != null && objRsp.Code == "00" && objRsp.Data != null)
                    {
                        return ResponseHelper.RspOK(objRsp.Data);
                    }
                    else
                    {
                        return ResponseHelper.RspNotWarning(400, objRsp.Code + "- " + objRsp.Desc ?? " trạng thái không hợp lệ");
                    }
                }
                else
                {
                    return ResponseHelper.RspNotWarning(400, "Lỗi api " + webApiInfo.Host.ToString() + " - vui lòng thử lại");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("CallApi O2 Exception: " + ex.Message.ToString());
                return ResponseHelper.RspNotWarning(9998, " VIETQR Exception" + ex.Message.ToString());
            }
        }
    }
}
