using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VCM.Shared.Dtos.POS;

namespace VCM.Shared.API
{
    public class RequestUpdateOrderStatus
    {
        [Required]
        [DefaultValue("PLG")]
        public string PartnerCode { get; set; } = "PLG";

        [Required]
        [DefaultValue("201801")]
        public string PosNo { get; set; } = "201801";

        [Required]
        [DefaultValue("1629631110753")]
        public string OrderNo { get; set; } = "1629631110753";

        [Required]
        [DefaultValue("0")]
        public decimal PaymentAmount { get; set; } = 0;

        [Required]
        [DefaultValue(1)]
        public int Status { get; set; } = 1;
        public IList<Remark> Remark { get; set; }
    }
}
