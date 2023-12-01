using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.Dtos.Invoice;
using WebApi.InvoiceNumbering.Models;
using WebApi.InvoiceNumbering.Services;

namespace WebApi.InvoiceNumbering.Controllers
{
    public class InvoiceController : BaseController
    {
        private readonly ILogger<InvoiceController> _logger;
        private readonly IInvoiceNumberingService _invoiceNumberingService;
        public InvoiceController
            (
                ILogger<InvoiceController> logger,
                IInvoiceNumberingService invoiceNumberingService
            )
        {
            _logger = logger;
            _invoiceNumberingService = invoiceNumberingService;
        }

        [HttpPost]
        [Route("api/invoice/numbering")]
        public InvoiceNumberingDto GetInvoiceNumbering([FromBody] InvoiceNumberingRequest request)
        {
            return _invoiceNumberingService.GetNextInvoiceNumber(request);
        }

    }
}
