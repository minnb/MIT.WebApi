using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.BLUEPOS
{
    public class CreateNewVoucherSAP
    {
        public string VoucherNumber { get; set; }
        public decimal Value { get; set; }
        public string From_Date { get; set; }
        public string Expiry_Date { get; set; }
        public string BonusBuy { get; set; }
        public string Article_No { get; set; }
        public string POSTerminal { get; set; }
        public string SiteCode { get; set; }
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public string RequestId { get; set; }
    }
}
