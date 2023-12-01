using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Const
{
    public static class KIOSConts
    {
        public static Dictionary<string, decimal> MappingTax()
        {
            Dictionary<string, decimal> openWith = new Dictionary<string, decimal>
            {
                { "2", 0 },
                { "3", 5 },
                { "4", 10 },
                { "5", 8 },
                { "6", 0 }
            };
            return openWith;
        }
    }
}
