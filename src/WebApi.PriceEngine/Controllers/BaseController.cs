﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.PriceEngine.Controllers
{
    [Authorize]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {

        }
    }
}