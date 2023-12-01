using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Voucher
{
    public class IssueVoucher
    {
        [Required]
        [RegularExpression(@"^\$?\d+(\.(\d{2}))?$")]
        [StringLength(11, MinimumLength = 9)]
        public string PhoneNumber { get; set; }
        [Required]
        [Range(1, 999, ErrorMessage = "Số lượng voucher chỉ từ 1 tới 999")]
        public int NumberOfVouchers { get; set; }
    }
}
