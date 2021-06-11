using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Utils.Helpers
{
    public class StringHelper
    {
        /// <summary>
        /// Completes the UnSign.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Complete Link</returns>
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
    }
}
