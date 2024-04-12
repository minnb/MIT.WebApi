using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VCM.Common.Helpers
{
    public static class StringHelper
    {
        private static Random random = new Random();
        public static string GetMonth(int backMonth, string format)
        {
            return DateTime.Now.AddMonths(backMonth).ToString(format);
        }
        public static string GetTimeStampString(bool isRandom = false)
        {
            string ramdom = string.Empty;
            if (isRandom)
            {
                ramdom = RandomString(2);
            }
            return ramdom + ((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }
        private static string Format_Json(string json)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);

            return System.Text.Json.JsonSerializer.Serialize(jsonElement, options);
        }
        public static string NumberPadLeft(long number, char c, int numberChar)
        {
            return number.ToString().PadLeft(numberChar, c);
        }
        public static string GetFirstSplit(string str, char c)
        {
            try
            {
                var arr = str.Split(c);
                return arr[0];
            }
            catch
            {
                return "";
            }
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static DateTime ConvertStringToDate(string date)
        {
            return DateTime.ParseExact(date, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date;
        }
        public static DateTime ConvertStringToDate(string date, string format = "")
        {
            return DateTime.ParseExact(date, format, System.Globalization.CultureInfo.InvariantCulture).Date;
        }
        public static string InitRequestId()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }
        public static string Right(string input, int count)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return input.Substring(input.Length - count, count);
            }
            else
            {
                return input;
            }
        }
        public static string DateToString(DateTime date)
        {
            if (date != null)
            {
                return date.ToString("yyyyMMdd");
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// Completes the UnSign.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Complete Link</returns>
        /// 
        public static string CompleteUnSign(string str)
        {
            str = RemoveSign4VietnameseString(str);
            str = RemoveSpecialWord(str);
            str = str.Trim();
            str = str.Replace("\"", string.Empty);
            str = str.ToLower();

            return str;
        }

        /// <summary>
        /// Completes the name file images.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Complete Name file Images</returns>
        public static string CompleteNamefileImages(string str)
        {
            str = RemoveSign4VietnameseString(str);
            str = RemoveSpecialWordForImages(str);
            str = str.Trim();
            str = str.Replace(" ", "-");
            str = str.Replace("\"", string.Empty);
            str = str.ToLower();

            return str;
        }
        public static string CompleteCategoryAdayroi(string str)
        {
            str = RemoveSign4VietnameseString(str);
            str = RemoveSpecialWord(str);
            str = str.Trim();
            str = str.Replace(" ", "");
            str = str.Replace("\"", string.Empty);
            str = str.ToLower();

            return str;
        }

        /// <summary>
        /// Removes the special word for images.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Remove Special Word For Images</returns>
        public static string RemoveSpecialWordForImages(string str)
        {
            string sp = @"~!@#$%^&*()+[]{}\/><,:;";
            int lenght;
            lenght = str.Length;
            string rt = string.Empty;
            for (int i = 0; i < lenght; i++)
            {
                bool p = true;
                for (int k = 0; k < sp.Length; k++)
                {
                    if (str[i] == sp[k])
                    {
                        p = false;
                        break;
                    }
                }

                if (p == true)
                {
                    rt = rt + str[i];
                }
            }

            return rt;
        }

        /// <summary>
        /// Completes the link.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Complete Link</returns>
        public static string CompleteLink(string str)
        {
            str = RemoveSign4VietnameseString(str);
            str = RemoveSpecialWord(str);
            str = str.Trim();
            str = str.Replace(" ", "-");
            str = str.Replace("\"", string.Empty);

            return str;
        }

        /// <summary>
        /// Removes the sign4 vietnamese string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Remove Sign4 Vietnamese String</returns>
        public static string RemoveSign4VietnameseString(string str)
        {
            ////Tiến hành thay thế , lọc bỏ dấu cho chuỗi

            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                {
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
                }
            }

            return str;
        }

        /// <summary>
        /// The vietnamese signs
        /// </summary>
        private static readonly string[] VietnameseSigns = new string[]
        {
            "aAeEoOuUiIdDyY",

            "áàạảãâấầậẩẫăắằặẳẵ",

            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",

            "éèẹẻẽêếềệểễ",

            "ÉÈẸẺẼÊẾỀỆỂỄ",

            "óòọỏõôốồộổỗơớờợởỡ",

            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",

            "úùụủũưứừựửữ",

            "ÚÙỤỦŨƯỨỪỰỬỮ",

            "íìịỉĩ",

            "ÍÌỊỈĨ",

            "đ",

            "Đ",

            "ýỳỵỷỹ",

            "ÝỲỴỶỸ"
        };

        /// <summary>
        /// Removes the special word.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Remove Special Word</returns>
        public static string RemoveSpecialWord(string str)
        {
            string sp = @"~!@#$%^&*()+[]{}\/><.,:;";
            int lenght;
            string rt = string.Empty;
            if (!string.IsNullOrEmpty(str))
            {
                lenght = str.Length;
                for (int i = 0; i < lenght; i++)
                {
                    bool p = true;
                    for (int k = 0; k < sp.Length; k++)
                    {
                        if (str[i] == sp[k])
                        {
                            p = false;
                            break;
                        }
                    }

                    if (p == true)
                    {
                        rt = rt + str[i];
                    }
                }
            }

            return rt;
        }

        /// <summary>
        /// Cuts the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="length">The length.</param>
        /// <returns>Cut String</returns>
        public static string CutString(string str, int length)
        {
            try
            {
                if (!string.IsNullOrEmpty(str) && str.Length > length)
                {
                    return str.Substring(0, length) + "...";
                }
                else
                {
                    return str;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string[] SliptString(string str, string characters)
        {
            try
            {
                if (str.IndexOf(characters) > 0)
                {
                    return str.Split(characters);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static string CutStringBuyChar(string str, string starChar, string endChar)
        {
            try
            {
                int charStart = str.IndexOf(starChar);
                int charEnd = str.IndexOf(endChar);
                if(charStart >= 0 && charEnd >= 0)
                {
                    return str.Substring(charStart, charEnd + 1);
                }
                else
                {
                    return "";
                }
                
            }
            catch
            {
                return "";
            }
        }
        public static string MakeVNtoEN(string str)

        {
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str;
        }
    }
}
