using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VCM.Shared.Entity.Partner;

namespace VCM.Common.Helpers
{
    public static class MessageHelper
    {
        public static string GetMsgConfig(List<NotifyConfig> messConfig, string type, string status)
        {
            string mess = string.Empty;
            try
            {
                if (messConfig != null && messConfig.Count > 0)
                {
                    if (!string.IsNullOrEmpty(mess))
                    {
                        mess = messConfig.FirstOrDefault(x => x.ActionType == type && x.Status == status) != null ? messConfig.FirstOrDefault(x => x.ActionType == type && x.Status == status).Message : mess;
                    }
                    else
                    {
                        mess = messConfig.FirstOrDefault(x => x.Status == status) != null ? messConfig.FirstOrDefault(x => x.Status == status).Message : mess;
                    }
                    //if (mess.ToUpper().Contains("{VoucherNo}".ToUpper()))
                    //{
                    //    try
                    //    {
                    //        StringBuilder builder = new StringBuilder(mess);
                    //        //builder.Replace("{VoucherNo}", voucherNo);
                    //        mess = builder.ToString();
                    //    }
                    //    catch
                    //    {

                    //    }
                    //}
                }
            }
            catch
            {

            }
            return mess;
        }
    }
}
