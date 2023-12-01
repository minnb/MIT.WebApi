using System.Collections.Generic;
using System.Linq;
using WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine;
using WebApi.PriceEngine.Enums;

namespace WebApi.PriceEngine.Helpers
{
    public static class PriceEngineConfigHelper
    {
        public static string GetFunctionSetting(List<SysStoreSet> lstStore, string OrderNo)
        {
            string function = string.Empty;
            string storeNo = OrderNo.Substring(0, 4);
            var subSet = lstStore.Where(x => x.StoreNo == storeNo).FirstOrDefault();

            if (OrderNo.Substring(0, 1) == "P")
            {
                function = PriceEngineAppCode.WIN_DR.ToString();
            }
            else if(subSet != null)
            {
                function = subSet.SubSet.ToString().ToUpper();
            }

            return function;
        }
        public static string GetAppCode(string OrderNo)
        {
            string appCode = string.Empty;
            if (OrderNo.Substring(0, 1) == "P")
            {
                appCode = PriceEngineAppCode.WIN_DR.ToString();
            }
            else
            {
                appCode = PriceEngineAppCode.WIN_LIFE.ToString();
            }
            return appCode;
        }
    }
}
