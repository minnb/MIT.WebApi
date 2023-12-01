using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Core.Common.Extentions
{
    public static class NetExtention
    {
        public static string GetComputerName()
        {
            string computer_name = System.Environment.MachineName;
            return computer_name[0].ToString();
        }
    }
}
