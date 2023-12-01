using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.BLUEPOS
{
    public class CheckVoucherSAP
    {
        [Required]
        public string PosNo { get; set; }
        [Required]
        public string PhoneNumber { get; set;}
    }
    public class RspCheckVoucherSAP
    {
        public string VoucherNumber { get; set; }
        public string Status { get; set; }
        public decimal Value { get; set; }
        public string ActicleType { get; set; }
        public string ActicleNo { get; set; }
        public string Expiry_Date { get; set; }
        public string Validity_From_Date { get; set; }
        public string CompanyCode { get; set; }
        public string PhoneNumber { get; set; }

    }
}
