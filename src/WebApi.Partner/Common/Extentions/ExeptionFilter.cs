using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace VCM.Partner.API.Common.Extentions
{
    public class ExeptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
