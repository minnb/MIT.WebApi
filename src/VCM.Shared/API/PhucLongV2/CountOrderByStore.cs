using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.PhucLongV2
{
    public class CountOrderRequest
    {
        [Required]
        [DefaultValue("PLH")]
        public string PartnerCode { get; set; } = "PLH";

        [Required]
        [DefaultValue("WEB")]
        public string AppCode { get; set; } = "WEB";

        [Required]
        [DefaultValue("2018")]
        public string StoreNo { get; set; } = "2018";
    }

    public class CountOrderResponse
    {
        public string PartnerCode { get; set; } = "PLH";
        public string AppCode { get; set; } = "WEB";
        public string StoreNo { get; set; } = "2018";
        public int Counted { get; set; }
    }
}
