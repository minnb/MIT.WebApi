using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Dtos.POS;

namespace VCM.Shared.API
{
    public class ResponseVoucher
    {
        public string SerialNumber { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public decimal VoucherAmount { get; set; }
        public string PublishDate { get; set; }
        public string EffectiveDateFrom { get; set; }
        public string EffectiveDateTo { get; set; }
        public string DateUsed { get; set; }
        public string UsedOn { get; set; }
        public Remark Remark { get; set; }
    }
}