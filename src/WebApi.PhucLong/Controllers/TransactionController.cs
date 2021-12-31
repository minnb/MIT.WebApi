using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using VCM.Shared.API.PLG;
using VCM.PhucLong.API.Services;
using System.Threading.Tasks;

namespace VCM.PhucLong.API.Controllers
{
    [ApiController]
    public class TransactionController: BaseController
    {
        private readonly ITransactionService _transactionService;
        public TransactionController
            (
                ITransactionService transactionService
            )
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        [Route("api/v1/transaction/order/id")]
        public async Task<ActionResult> GetOrderDetailById([Required] string order_no = "CH8301-30035-006-0021", [Required] int location_id = 1387, [Required] int set = 1)
        {
            var result = await _transactionService.GetOrderByIdAsync(set, order_no, location_id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("api/v1/transaction/order")]
        public async Task<ActionResult> GetOrderDetail([Required] string order_no = "KVLADPDP01-17558-001-0001", [Required] int location_id = 1304, [Required] int set = 1)
        {
            var result = await _transactionService.GetOrderAsync(set, order_no, location_id);
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
        [Route("api/v1/transaction/order/list")]
        public async Task<ActionResult> GetOrderListAsync([FromBody] RequestOrderList request)
        {
            var result = await _transactionService.GetOrderListAsync(request);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("api/v1/transaction/order/update-status")]
        public ActionResult StatusOrder([Required] string order_no = "CH45-POS04-06574-001-0010", [Required] int status = 1, [Required] int set = 1)
        {
            if (_transactionService.UpdateStatusOrderAsync(set, order_no, status))
            {
                return Ok("OK");
            }
            else
            {
                return NotFound();
            }
        }
    }
}
