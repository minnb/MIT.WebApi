using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VCM.Shared.Const;
using VCM.Shared.Dtos.POS;

namespace VCM.Shared.API
{
    public class RequestUpdateOrderStatus
    {
        [Required]
        [DefaultValue("PLH")]
        public string PartnerCode { get; set; } = "PLH";

        public string AppCode { get; set; }

        [Required]
        [DefaultValue("200101")]
        public string PosNo { get; set; } = "200101";

        [Required]
        [DefaultValue("PLH1652759636107")]
        public string OrderNo { get; set; } = "PLH1652759636107";

        [Required]
        [DefaultValue("0")]
        public decimal PaymentAmount { get; set; } = 0;

        [Required]
        [DefaultValue(1)]
        public int Status { get; set; } = 1;
        public IList<Remark> Remark { get; set; }
    }
}
