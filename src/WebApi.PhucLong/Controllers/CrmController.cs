using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.PhucLong.API.Controllers;
using VCM.Shared.API.PLG;
using VCM.Shared.Entity.PhucLong.Dtos;
using WebApi.PhucLong.Services;

namespace WebApi.PhucLong.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [DisplayName("Odoo")]
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

        [HttpPost]
        [Route("api/v1/odoo/coupon")]
        public async Task<ActionResult> CheckVoucherOdoo(RequestVoucherCheckOdoo request)
        {
            var result = await _crmService.CheckVoucherOdoo(request);
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
        [Route("api/v1/odoo/coupon/redeem")]
        public async Task<ActionResult> VoucherRedeemOdoo(RequestVoucherRedeemOdoo request)
        {
            var result = await _crmService.VoucherRedeemOdoo(request);
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
