using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Const
{
    public static class AppPartnerPLHConst
    {
        public static string GetPartnerCode(string appCode)
        {
            Dictionary<string, string> openWith = new Dictionary<string, string>()
            {
                //get partnerCode from AppCode
                { "CRX", "PLH" },
                { "SHOPEE", "PLH" },
                { "NOWFOOD", "PLH" },
                { "PLH", "PLH" },
                { "WCM", "WCM" },
                { "WMP", "WCM" },
                { "VMP", "WCM" },
                { "VMT", "WCM" },
                { "TVB", "TVB" },
                { "PLF", "PLH" },
                { "WIN", "WCM" }
            };

            return openWith[appCode];

        }

    }
}
