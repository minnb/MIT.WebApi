using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API
{
    public class RequestTransaction
    {
        [Required]
        [DefaultValue("MBC")]
        public string PartnerCode { get; set; } = "WMT";

        [Required]
        [DefaultValue("2018")]
        public string StoreNo { get; set; } = "2018";

        [Required]
        [DefaultValue("201801")]
        public string PosNo { get; set; } = "201801";

        [Required]
        [DefaultValue("1630307833376")]
        public string OrderNo { get; set; } = "1630307833376";
    }
}
