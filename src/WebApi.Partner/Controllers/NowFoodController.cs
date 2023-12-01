using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using VCM.Partner.API.Application.Interfaces;
using VCM.Shared.API.Shopee.Webhooks;
using VCM.Shared.Entity.Partner.Shopee;

namespace WebApi.Partner.Controllers
{
    public class NowFoodController : ControllerBase
    {
        private readonly ILogger<NowFoodController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IShopeeService _shopeeService;
        private string _proxyHttp = "";
        private string[] _bypassList;
        public NowFoodController
        (
             IConfiguration configuration,
             ILogger<NowFoodController> logger,
             IShopeeService shopeeService
         )
        {
            _configuration = configuration;
            _logger = logger;
            _shopeeService = shopeeService;
            if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
            {
                _proxyHttp = _configuration["WebProxy:Http"];
                _bypassList = new string[] { _configuration["WebProxy:Bypasslist"] };
            }
        }
        
        [HttpPost]
        [Route("api/foody/partner_api_url_callback/update_order")]
        public async Task<ActionResult> Shopee_update_order( [FromBody] Partner_api_url_callback_update_order request)
        {
            _logger.LogWarning("===>ShopeeFood partner_api_url_callback_update_order: " + JsonConvert.SerializeObject(request));
            if (ModelState.IsValid)
            {
                if (await _shopeeService.Shopee_update_order(request))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("api/foody/partner_api_url_callback")]
        public async Task<ActionResult> Shopee_update_menu([FromBody] Partner_api_url_callback request)
        {
            _logger.LogWarning("===>ShopeeFood partner_api_url_callback_menu: " + JsonConvert.SerializeObject(request));
            if (await _shopeeService.Shopee_update_menu(request))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("api/foody/export_error_menu")]
        public async Task<ActionResult> Shopee_export_error_menu([FromBody] export_error_menu request)
        {
            _logger.LogWarning("===>ShopeeFood shopee_export_error_menu: " + JsonConvert.SerializeObject(request));
            if (await _shopeeService.Shopee_export_error_menu(request))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("api/foody/partner_api_url_callback/update_drivers_status")]
        public async Task<ActionResult> Shopee_update_drivers_status([FromBody]  partner_api_url_callback_update_drivers_status request)
        {
            _logger.LogWarning("===>ShopeeFood shopee_update_drivers_status: " + JsonConvert.SerializeObject(request));
            if (await _shopeeService.Shopee_update_drivers_status(request))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
