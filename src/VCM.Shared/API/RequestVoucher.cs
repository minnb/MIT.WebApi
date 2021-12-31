using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API
{
    public class RequestVoucher
    {
        [Required]
        [DefaultValue("PLG")]
        public string PartnerCode { get; set; } = "PLG";

        [Required]
        [DefaultValue("999901")]
        public string PosNo { get; set; } = "999901";

        [Required]
        [DefaultValue("9912310000002")]
        public string SerialNumber { get; set; } = "9912310000002";

        [Required]
        [DefaultValue("SOLD")] //REDE
        public string Status { get; set; } = "SOLD";

        [DefaultValue("20211020")]
        public string EffectiveDateFrom { get; set; } = "20211020";

        [DefaultValue("20211231")]
        public string EffectiveDateTo { get; set; } = "20211231";

        [Range(1, 999999)]
        [DefaultValue(20000)]
        public decimal VoucherAmount { get; set; } = 20000;
        public string OrderReference { get; set; }
    }
}
