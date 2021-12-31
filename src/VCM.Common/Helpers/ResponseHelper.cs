using System;
using VCM.Shared.API;

namespace VCM.Common.Helpers
{
    public static class ResponseHelper
    {
        public static ResponseClient RspNotWarning(int statusCode, string message)
        {
            return new ResponseClient()
            {
                Meta = new Meta
                {
                    Code = statusCode,
                    Message = message
                },
                Data = null
            };
        }
        public static ResponseClient RspNotFoundData(string message)
        {
            return new ResponseClient()
            {
                Meta = new Meta
                {
                    Code = 400,
                    Message = message
                },
                Data = null
            };
        }
        public static ResponseClient RspOK(object data)
        {
            return new ResponseClient()
            {
                Meta = new Meta
                {
                    Code = 200,
                    Message = "OK"
                },
                Data = data
            };
        }
        public static ResponseClient RspNotAuth(string partner)
        {
            return new ResponseClient()
            {
                Meta = new Meta
                {
                    Code = 401,
                    Message = partner + " lỗi đăng nhập Api"
                },
                Data = null
            };
        }
        public static Meta RspTimeOut(string partner)
        {
            return new Meta
            {
                Code = 408,
                Message = partner + " request timeout"
            };
        }
        public static ResponseClient RspTryCatch(Exception ex)
        {
            string messageError = ex.Message.ToString();
            if (ex.InnerException != null)
            {
                messageError += @"====> " + ex.InnerException.Message.ToString();
            }
            return new ResponseClient()
            {
                Meta = new Meta()
                {
                    Code = 9999,
                    Message = messageError
                },
                Data = null
            };
        }
        public static ResponseClient RspWarnByPartner(string statusCode, string mess)
        {
            return new ResponseClient()
            {
                Meta = new Meta()
                {
                    Code = int.TryParse(statusCode, out int status) ? status : 400,
                    Message = string.Format("{0} {1}", statusCode, mess)
                },
                Data = null
            };
        }
        public static Meta MetaOK(int statusCode, string mess)
        {
            return new Meta()
            {
                Code = statusCode,
                Message = mess
            };
        }
    }
}
