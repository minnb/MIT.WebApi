using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Shared.Dtos.POS
{
    public class TransPaymentEntryDto
    {
        public int LineNo { get; set; }
        public string TenderType { get; set; }
        public decimal PaymentAmount { get; set; }
        public string ReferenceNo { get; set; }
        public string TransactionId { get; set; }
        public string PayForOrderNo { get; set; }
        public string TransactionNo { get; set; }
        public string ApprovalCode { get; set; }
        public string TraceCode { get; set; }
        public object AdditionalData { get; set; }
    }

}
