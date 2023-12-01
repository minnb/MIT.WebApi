using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.PriceEngine.Models.Master
{
    public class TableNameMasterModel
    {
        public string AppCode { get; set; }
        public string TableName { get; set; }
        public int MaxCounter { get; set; }
    }
}
