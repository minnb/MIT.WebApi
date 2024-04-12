using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Common.Helpers
{
    public static class NumberHelper
    {
        public static decimal StrToDecimal(string str)
        {
            if (decimal.TryParse(str, out decimal result))
            {
                
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static double StrToDouble(string str)
        {
            if (double.TryParse(str, out double result))
            {

            }
            else
            {
                result = 0;
            }
            return result;
        }
    }
}
