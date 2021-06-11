using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MIT.Dtos;
using MIT.EntityFrameworkCore;
using MIT.WebApi.GPAY.ViewModels.AirPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.WebApi.GPAY.Controllers
{
    //[ApiController]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {

        }
    }
}
