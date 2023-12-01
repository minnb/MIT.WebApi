using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace VCM.Common.Helpers
{
    public static class XmlHelper
    {
        public static XmlDocument ReadXml(string jobName, string fileName)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(File.ReadAllText(fileName));
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(jobName, "FileName: " + fileName.ToString() + "|@| Exception:" + ex.Message.ToString());
            }

            return doc;
        }
        public static void ReplaceXml(string str, string strRegex)
        {
            //string strRegex = @"Thread-" + @"[0-9]*" + " platform.services.LoyaltyAdapter - Response received from web service:";
            if (str.ToLower().Contains(strRegex))
            {
                Regex.Replace(str, strRegex, "");
            }
        }

    }
}
