using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Invoice
{
    [Table("InvoiceNumbering")]
    public class InvoiceNumbering
    {
        public int Id { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public string TemplateNo { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public int StartNumber { get; set; }
        public int EndNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Blocked { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
