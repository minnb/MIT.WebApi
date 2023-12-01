using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.BLUEPOS
{
    public class RspCreateNewVoucherSAP
    {
        public string Status { get; set; }
        public string Return { get; set; }
        public string ActicleNo { get; set; }
        public string ActicleType { get; set; }
        public string VoucherNumber { get; set; }
        public string Value { get; set; }
        public string Voucher_Currency { get; set; }
        public string Validity_From_Date { get; set; }
        public string Expiry_Date { get; set; }
        public string CompanyCode { get; set; }
        public string Partner { get; set; }
        public string IsEmployee { get; set; }
    }
}
