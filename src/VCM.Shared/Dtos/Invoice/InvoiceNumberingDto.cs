using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.Invoice
{
    public class InvoiceNumberingDto
    {
        public string StoreNo { get; set; }
        public string PosNo { get; set; }
        public string TaxCode { get; set; }
        public string SerialNo { get; set; }
        public string TemplateNo { get; set; }
        public string InvoiceNumber { get; set; }
    }
}
