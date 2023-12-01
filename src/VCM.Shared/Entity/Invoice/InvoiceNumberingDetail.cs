using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Invoice
{
    [Table("InvoiceNumberingDetail")]
    public class InvoiceNumberingDetail
    {
        public string StoreNo { get; set; } = string.Empty;
        public string PosNo { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string TemplateNo { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string IssueDateTime { get; set; }
        public string AddressIp { get; set; } = string.Empty;
    }
}
