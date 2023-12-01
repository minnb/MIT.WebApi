using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.PriceEngine.Models.Master
{
    public class BarcodesMasterModel
    {
        public string AppCode { get; set; }
        public string BarcodeNo { get; set; }
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public bool Blocked { get; set; }
        public int Counter { get; set; }
    }
}
