using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.PriceEngine.Models.Master
{
    public class ItemMasterModel
    {
        public string AppCode { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public string TaxGroupCode { get; set; }
        public int VatPercent { get; set; }
        public string MCH { get; set; }
        public string ItemType { get; set; }
        public string Size { get; set; }
        public bool Blocked { get; set; }
        public int Counter { get; set; }
    }
}
