using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Common.Helpers
{
    public static class MathHelper
    {
        public static decimal CalcNetAmount(decimal totalAmount, decimal vatPercent)
        {           
            return  totalAmount > 0 ? Math.Round(totalAmount / (1 + vatPercent / 100), 0) : 0;
        }
        public static decimal CalcVATAmount(decimal totalAmount, decimal vatPercent)
        {
            if(totalAmount > 0)
            {
                return totalAmount - Math.Round(totalAmount / (1 + vatPercent / 100), 0);
            }
            else
            {
                return 0;
            }
        }
    }
}
