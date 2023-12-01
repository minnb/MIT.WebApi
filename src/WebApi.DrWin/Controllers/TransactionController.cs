using Microsoft.AspNetCore.Mvc;

namespace WebApi.DrWin.Controllers
{
    public class TransactionController: BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return Ok();
        }
    }
}
