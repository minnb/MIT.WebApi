using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VCM.Shared.Dtos.POS;

namespace VCM.Shared.API.O2
{
    public class UpdateTierMemberO2
    {
        [Required]
        [DefaultValue("O2")]
        public string PartnerCode { get; set; } = "O2";

        [Required]
        [DefaultValue("373701")]
        public string PosNo { get; set; } = "373701";

        [Required]
        [DefaultValue("O2_UPDATE_TIER")]
        public string ServiceType { get; set; } = "O2_UPDATE_TIER";

        [Required]
        [DefaultValue("0559900185")]
        public string MemberNumber { get; set; } = "0559900185";

        [Required]
        [DefaultValue("VIP")]
        public string DataUpdate { get; set; } = "VIP";

        public IList<Remark> Remark { get; set; }
    }
}
