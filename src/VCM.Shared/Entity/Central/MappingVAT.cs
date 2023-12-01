using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    public class MappingVAT
    {
        public string AppCode { get; set; }
        public int Code { get; set; }
        public string CodeSAP { get; set; }
        public string VatGroup { get; set; }
        public decimal VatPercent { get; set; }
        public int TaxId { get; set; }
        public string Description { get; set; }
        public bool Blocked { get; set; }
    }
}
