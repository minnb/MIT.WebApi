using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    public class MappingTender
    {
        public string AppCode { get; set; }
        public string Type { get; set; }
        public string WCM { get; set; }
        public string TenderType { get; set; }
        public string Description { get; set; }
        public bool Blocked { get; set; }
    }
}
