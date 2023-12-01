using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Wintel
{
    public class CheckExtendSubscriberInfoRequest
    {
        [Required]
        [DefaultValue("MBC")]
        public string PartnerCode { get; set; } = "MBC";

        [Required]
        [DefaultValue("KIT_WINTEL_WIN99")]
        public string ServiceType { get; set; } = "KIT_WINTEL_WIN99";

        [Required]
        [DefaultValue("373701")]
        public string PosNo { get; set; } = "373701";

        [Required]
        [DefaultValue("89840905510000099835")]
        public string Serial { get; set; } = "89840905510000099835";
    }
}
