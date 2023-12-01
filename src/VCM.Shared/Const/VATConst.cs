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
                { 0, 0 },
                { 1, 0 },
                { 2, 0 },
                { 3, 5 },
                { 4, 10 },
                { 5, 5 },
                { 9, 0 }
            };
            return openWith;
        }
        public static Dictionary<int, string> MappingProductSize()
        {
            Dictionary<int, string> openWith = new Dictionary<int, string>
            {
                { 0, "" },
                { 1, "M" },
                { 2, "L" },
                { 3, "Hot" }
            };
            return openWith;
        }
        public static Dictionary<int, decimal> MappingTaxWCM()
        {
            Dictionary<int, decimal> openWith = new Dictionary<int, decimal>
            {
                { 0, 0 },
                { 1, 0 },
                { 2, 0 },
                { 3, 5 },
                { 4, 10 },
                { 5, 5 },
                { 6, 8 },
                { 9, 0 }
            };
            return openWith;
        }
        public static Dictionary<int, decimal> MappingTaxPLH()
        {
            Dictionary<int, decimal> openWith = new Dictionary<int, decimal>
            {
                { 2, 0 },
                { 3, 5 },
                { 4, 10 },
                { 5, 8 },
                { 6, 0 },
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