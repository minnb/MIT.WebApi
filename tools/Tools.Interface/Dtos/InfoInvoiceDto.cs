using System;
using System.Collections.Generic;
using System.Text;

namespace Hifresh.Interface.Dtos
{
    public class InfoInvoiceDto
    {
        public string CustomerName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string TaxCode { get; set; }
    }

    public class InvoiceCreated: InfoInvoiceDto
    {
        public string OrderNo { get; set; }
        public string StoreNo { get; set; }
    }
}
