using System;
using VCM.Shared.API;

namespace VCM.Partner.API.Common.Const
{
    public static class ExceptionConst
    {
        public static Meta ExceptionTryCatch(Exception ex)
        {
            string messageError = ex.Message.ToString();
            if (ex.InnerException != null)
            {
                messageError += @"====> " + ex.InnerException.Message.ToString();
            }
            return new Meta()
            {
                Code = 9999,
                Message = messageError
            };
        }
    }
}
