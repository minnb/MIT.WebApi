using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.PhucLong.API.Controllers;
using VCM.Shared.API.PLG;
using VCM.Shared.Entity.PhucLong.Dtos;
using WebApi.PhucLong.Services;

namespace WebApi.PhucLong.Controllers
{
    [ApiController]
    public class CrmController : BaseController
    {
        private readonly ICrmService _crmService;
        public CrmController
            (
                ICrmService crmService
            )
        {
            _crmService = crmService;
        }

        [HttpGet]
        [Route("api/v1/crm/voucher")]
        public async Task<ActionResult> GetOrderDetail([Required] string serial_number = "9912310000008")
        {
            var result = await _crmService.CheckVoucher(serial_number);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("api/v1/crm/voucher/update-status")]
        public async Task<ActionResult> GetOrderListAsync([FromBody] RequestVoucherPLG request)
        {
            if (ModelState.IsValid)
            {
                var result1 = await _crmService.UpdateStatusVoucher(request).ConfigureAwait(false);
                if (result1 != null)
                {
                    return Ok(result1);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }           
        }
    }
}
