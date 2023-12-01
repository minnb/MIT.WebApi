using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API
{
    public class RequestListOrderPOS
    {
        [Required]
        [DefaultValue("PLH")]
        public string PartnerCode { get; set; } = "PLH";
        [DefaultValue("SHOPEE")]
        public string AppCode { get; set; } = "SHOPEE";

        [Required]
        [DefaultValue("200101")]
        public string PosNo { get; set; } = "200101";

        [DefaultValue("yyyyMMdd")]
        public string FromDate { get; set; }

        [DefaultValue("yyyyMMdd")]
        public string ToDate { get; set; }

        [DefaultValue("Tên khách")]
        public string CustomerKeyword { get; set; }

        [DefaultValue("Tên sản phẩm")]
        public string ItemKeyword { get; set; }

        [Required]
        [DefaultValue("1")]
        public int PageNumber { get; set; } = 1;

        [Required]
        [DefaultValue("10")]
        public int PageSize { get; set; } = 10;
    }
}
