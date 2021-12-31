using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.POS
{
    public class LoyaltyDto
    {
        public string QRCode { get; set; }
        public string CardNumber { get; set; }
        public string MerchantId { get; set; }
        public string TerminalId { get; set; }
        public string InvoiceNo { get; set; }
        public string OrderNo { get; set; }
        public int SpendPoints { get; set; }
        public decimal BillAmount { get; set; }
        public decimal OrderAmount { get; set; }
        public bool IsOffline { get; set; }
        public string VirtualCard { get; set; }
    }
}
