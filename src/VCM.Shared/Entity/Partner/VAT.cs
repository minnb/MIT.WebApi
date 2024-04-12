using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Partner
{
    [Table("M_VAT")]
    public class VAT
    {
        public string AppCode { get; set; }
        public string TaxGroupCode { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        public decimal VatPercent { get; set; }
        public bool Blocked { get; set; }
    }
}
