using System;
using System.Collections.Generic;
using System.Text;
using Tools.Interface.Models;
using VCM.Shared.Entity.Partner;

namespace Tools.Interface.Helpers
{
    public static class WebApiInfoHelper
    {
        public static WebApiInfo WebApiInfoResult(SysWebApi header, List<SysWebRoute> detail)
        {
            return new WebApiInfo()
            {
                AppCode = header.AppCode,
                Host = header.Host,
                Description = header.Description,
                UserName = header.UserName,
                Password = header.Password,
                PublicKey = header.PublicKey,
                PrivateKey = header.PrivateKey,
                Blocked = header.Blocked,
                HttpProxy = header.HttpProxy,
                Bypasslist = header.Bypasslist,
                WebRoute = detail
            };
        }
    }
}
