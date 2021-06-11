using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.WebApi.GPAY.ViewModels.AirPay
{
    public class POSPurchaseCardRequest
    {
        [Required]
        [DefaultValue("153501")]
        public string Brand_id { get; set; } = "153501";
        [Required]
        [DisplayFormat(DataFormatString = "GPPC.100")]
        [DefaultValue("GPPC.100")]
        public string Service_code { get; set; } = "GPPC.100";
        [Required]
        [Range(1, 10000)]
        [DefaultValue(1)]
        public int Quantity { get; set; } = 1;
    }

    public class PurchaseCardAirPay : PurchaseCard
    {
        public string signature { get; set; }
    }
    public class PurchaseCard
    {
        public string service_code { get; set; }
        public string quantity { get; set; }
        public string reference_no { get; set; }
        public string partner_id { get; set; }
        public string brand_id { get; set; }

    }
}
