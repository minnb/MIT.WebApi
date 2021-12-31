using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VCM.Partner.API.Controllers
{
    
    [Authorize]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {

        }
    }
}
