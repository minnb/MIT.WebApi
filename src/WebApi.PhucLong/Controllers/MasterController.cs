using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.PhucLong.API.Services;

namespace VCM.PhucLong.API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class MasterController : BaseController
    {
        private readonly IMasterService _masterService;
        public MasterController
            (
                IMasterService masterService
            )
        {
            _masterService = masterService;
        }

        [HttpGet]
        [Route("api/v1/master/pos-config")]
        public ActionResult GetPosConfig([FromHeader] string pos_name = null, [FromHeader] int set = 3)
        {
            var result = _masterService.GetPosConfig(pos_name, set);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
