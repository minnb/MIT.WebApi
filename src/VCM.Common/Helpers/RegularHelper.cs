using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VCM.Common.Helpers
{
    public static class RegularHelper
    {
        public static bool IsPhoneNumber(string number)
        {
            return Regex.Match(number, @"^(\+[0-9]{9})$").Success;
        }
        public static string RemoveNonNumeric(this string phone)
        {
            try
            {
                return Regex.Replace(phone, @"[^0-9]+", "");
            }
            catch
            {
                return "";
            }
        }
        public static bool ValidatePhoneNumber(this string phone, bool IsRequired)
        {
            try
            {
                if (string.IsNullOrEmpty(phone) & !IsRequired)
                    return true;

                if (string.IsNullOrEmpty(phone) & IsRequired)
                    return false;

                var cleaned = phone.RemoveNonNumeric().Replace(" ", "");
                if (cleaned.Substring(0, 2) == "84")
                {
                    cleaned = "0" + cleaned.Substring(2, cleaned.Length - 2);
                }

                if (IsRequired)
                {
                    if (cleaned.Length == 10)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (cleaned.Length == 0)
                        return true;
                    else if (cleaned.Length > 0 & cleaned.Length < 10)
                        return false;
                    else if (cleaned.Length == 10)
                        return true;
                    else
                        return false; // should never get here
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
