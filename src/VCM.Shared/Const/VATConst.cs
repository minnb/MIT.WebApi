using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Const
{
    public static class VATConst
    {
        public static Dictionary<int, decimal> MappingTax()
        {
            Dictionary<int, decimal> openWith = new Dictionary<int, decimal>
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 5 },
                { 4, 10 },
                { 5, 5 },
                { 9, 0 }
            };
            return openWith;
        }
    }
}
/*
2 VAT IN 0%
3 VAT IN 5%
4 VAT IN 10%
 */