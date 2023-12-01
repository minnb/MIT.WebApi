using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PriceEngine
{
    [Table("TmpTransHeader")]
    public class TmpTransHeader
    {
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime CrtDate { get; set; }
        public string RequestId { get; set; }
        public bool IsLoyalty { get; set; }
        public string WinCode { get; set; }
    }
}
