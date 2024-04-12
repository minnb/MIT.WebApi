using Microsoft.AspNetCore.Mvc;
using System;
using VCM.Shared.API;
using WebApi.Core.AppServices.JWT;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using VCM.Partner.API.Application.Interfaces;
using YamlDotNet.Core.Tokens;

namespace WebApi.Partner.Controllers
{
    public class AuthController : ControllerBase
    {
        protected IConfiguration Configuration => HttpContext.RequestServices.GetService<IConfiguration>();
        private IGenerateTokenService _generateTokenService;
        private IMemoryCacheService _memoryCacheService;
        public AuthController(IGenerateTokenService generateTokenService, IMemoryCacheService memoryCacheService)
        {
            _generateTokenService = generateTokenService;
            _memoryCacheService = memoryCacheService;
        }

        [HttpPost]
        [Route("api/v1/auth/token")]
        public IActionResult CreateToken([FromBody] JwtUser jwtUser)
        {
            try
            {
                string _secretKey_jwt = Configuration["Jwt:Key"];
                string _issuer_jwt = Configuration["Jwt:Issuer"];
                string _audience_jwt = Configuration["Jwt:Audience"];
                int _expirationTime_jwt = int.Parse(Configuration["Jwt:Exp"]);

                if (!ModelState.IsValid)
                {
                    string _message = ModelState.Values.First().Errors[0].ErrorMessage.ToString();
                    return BadRequest(new { _message });
                }
                var checkUser = _memoryCacheService.GetUsers().Result;

                if(checkUser != null && checkUser.FirstOrDefault(x=>x.UserName == jwtUser.UserName && x.Password == jwtUser.Password) != null)
                {
                   var _token = _generateTokenService.GenerateToken(_secretKey_jwt, _issuer_jwt, _audience_jwt, _expirationTime_jwt, jwtUser.UserName);
                   return Ok(new { _token });
                }
                else
                {
                    string _message = "UserName hoặc Password không đúng";
                    return Ok(new { _message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex });
            }
        }
    }
}
