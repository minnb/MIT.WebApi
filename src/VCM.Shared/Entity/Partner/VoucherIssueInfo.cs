using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VCM.Shared.API.Voucher;

namespace VCM.Shared.Entity.Partner
{
    [Table("POS_VoucherIssueInfo")]
    public class VoucherIssueInfo : IssueVoucher
    {
        public Guid Id { get; set; }
        public string UpdateFlg { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime ChgDate { get; set; }
    }
}
