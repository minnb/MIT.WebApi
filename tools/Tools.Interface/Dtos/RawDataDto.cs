using ShopInShop.Interface.Models.Central;
using System.Collections.Generic;

namespace Hifresh.Interface.Dtos
{
    public class RawDataDto
    {
        public bool IssueInvoice { get; set; }
        public InfoInvoiceDto InfoInvoice { get; set; }
        public TransHeaderCentral TransHeader { get; set; }
        public List<TransLineCentral> TransLine { get; set; }
        public List<TransPaymentEntryCentral> TransPaymentEntry { get; set; }
    }

}
