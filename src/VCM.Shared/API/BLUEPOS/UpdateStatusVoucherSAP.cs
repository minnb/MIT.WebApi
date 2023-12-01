using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.BLUEPOS
{
    public class UpdateStatusVoucherSAP
    {
        [Required]
        public string PosNo { get; set; }
        [Required]
        public string OrderNo { get; set; }
        [Required]
        public string Status { get; set; }
        public string[] Vouchers { get; set; }
    }

    public class RequestUpdateStatusVoucherSAP
    {
        public string OrderNo { get; set; }
        public decimal TotalBill { get; set; }
        [Required]
        public string SiteCode { get; set; }
        public string POSTerminal { get; set; }
        public List<ListSeriNoUpdate> ListSeriNo { get; set; }
    }

    public class ListSeriNoUpdate
    {
        public string CompanyCode { get; set; }
        public string Partner { get; set; }
        public string VoucherNumber { get; set; }
        public bool IsVoucher { get; set; }
        public decimal Value { get; set; }
        public string ArticleNo { get; set; }
        public string ArticleType { get; set; }
        public string Status { get; set; }
    }
}
