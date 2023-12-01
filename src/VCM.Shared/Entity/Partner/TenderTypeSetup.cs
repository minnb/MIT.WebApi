using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Partner
{
    [Table("M_TenderType")]
    public class TenderTypeSetup
    {
        public string PartnerCode { get; set; }
        public string AppCode { get; set; }
        public string TenderType { get; set; }
        public string TenderTypeName { get; set; }
        public bool IsRefund { get; set; }
        public bool Blocked { get; set; }
    }
}
