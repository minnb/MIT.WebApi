using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.DrWin.Controllers
{
    [Authorize]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {

        }
    }
}
