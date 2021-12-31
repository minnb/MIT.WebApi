using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.PLG
{
    public class RequestVoucherPLG
    {
        [Required]
        [DefaultValue("9912310000002")]
        public string serial_number { get; set; } = "9912310000002";

        [Required]
        [DefaultValue("voucher")]
        public string type { get; set; } = "voucher";

        [Required]
        [DefaultValue("SOLD")] //REDE
        public string status { get; set; } = "SOLD";

        [DefaultValue("20211020")]
        public string effective_date_from { get; set; } = "20211020";

        [DefaultValue("20211231")]
        public string effective_date_to { get; set; } = "20211231";

        [Range(1, 999999)]
        [DefaultValue(20000)]
        public decimal amount { get; set; } = 20000;

        [DefaultValue("KVHCM71501-17633-001-0003")]
        public string pos_reference { get; set; } = "KVHCM71501-17633-001-0003";
    }
}
