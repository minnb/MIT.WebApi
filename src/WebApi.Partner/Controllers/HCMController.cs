using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1.Ocsp;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VCM.Common.Helpers;
using VCM.Partner.API.Controllers;
using VCM.Shared.API;
using WebApi.Core.AppServices.JWT;
using WebApi.Partner.Application.Implementation;

namespace WebApi.Partner.Controllers
{
    public class HCMController: BaseController
    {
        private readonly IHCMService _HCMService;
        public HCMController
            (
                IHCMService HCMService
            )
        {
            _HCMService = HCMService;
        }

        [HttpGet("api/v1/hcm/empl")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public ResponseClient GetEmplInfo([Required] string userName)
        {
            if (!this.ModelState.IsValid)
            {
                return ResponseHelper.RspNotWarning(400, LogInvalidModelState(ModelState));
            }
            
            var emplInfo = _HCMService.GetEmployeeInfo(userName);
            if(emplInfo.Item1 == HttpStatusCode.OK)
            {
                return ResponseHelper.RspOK(emplInfo.Item3);
            }
            
            return new ResponseClient()
            {
                Meta = new Meta()
                {
                    Code = (int)emplInfo.Item1,
                    Message = emplInfo.Item2
                },
                Data = emplInfo.Item3
            };
        }

        [HttpGet("validate")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Validate()
        {
            var user = GetJwtData();
            if (!string.IsNullOrEmpty(user.UserName))
            {
                return Ok(new { message = "Token is valid" });
            }
            else
            {
                return Ok(new { message = "Token is not valid" });
            }            
        }
    }
}
