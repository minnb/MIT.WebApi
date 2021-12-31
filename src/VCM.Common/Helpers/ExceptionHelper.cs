using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Common.Helpers
{
    public static class ExceptionHelper
    {
        public static void WriteExptionError(string func, Exception ex)
        {
            string messageError = "";
            if (ex.InnerException != null)
            {
                messageError = ex.InnerException.ToString();
            }
            messageError += " ====>>> " + ex.Message.ToString();
            FileHelper.WriteLogs(func + " Exception:" + messageError);
        }
    }
}
