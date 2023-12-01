using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    public class MappingStore
    {
        public string AppCode { get; set; }
        public string Type { get; set; }
        public string StoreNo { get; set; }
        public string StoreNo2 { get; set; }
        public string TenderType { get; set; }
        public decimal Discount { get; set; }
        public bool Blocked { get; set; }
    }
}
