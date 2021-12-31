using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.ViewModels.AirPay
{
    public class POSTopupRequest
    {
        [Required]
        [DefaultValue("MBC")]
        public string PartnerCode { get; set; } = "MBC";

        [Required]
        [DefaultValue("373701")]
        public string PosNo { get; set; } = "373701";

        [Required]
        [DefaultValue("153301210605119")]
        public string OrderNo { get; set; } = "153301210605119";

        [Required]
        [DefaultValue("0559384959")]
        public string Receiver { get; set; } = "0559384959";

        [Required]
        [Range(10000, 10000000)]
        [DefaultValue(10000)]
        public int TopupValue { get; set; } = 10000;
    }
    public class POSPurchaseCardRequest
    {
        [Required]
        [DefaultValue("MBC")]
        public string PartnerCode { get; set; } = "MBC";

        [Required]
        [DefaultValue("373701")]
        public string PosNo { get; set; } = "373701";
        [Required]
        [DefaultValue("153301210605119")]
        public string OrderNo { get; set; } = "153301210605119";
        
        [Required]
        [DefaultValue("PURCHASE_CARD")]
        public string ServiceType { get; set; } = "PURCHASE_CARD";

        [Required]
        [DefaultValue("GPPC.100")]
        public string ServiceCode { get; set; } = "GPPC.100";

        [Required]
        [Range(9999, 10000000)]
        [DefaultValue(10000)]
        public int PurchaseValue { get; set; } = 10000;

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