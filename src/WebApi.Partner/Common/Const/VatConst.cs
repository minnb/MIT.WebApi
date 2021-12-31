using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.Common.Const
{
    public static class VatConst
    {
        public static Dictionary<string, decimal> MappingTax()
        {
            Dictionary<string, decimal> openWith = new Dictionary<string, decimal>
            {
                { "1", 0 },
                { "2", 0 },
                { "3", 5 },
                { "4", 10 },
                { "5", 5 },
                { "9", 0 },
                { "11", 10 }
            };
            return openWith;
        }
    }
}
