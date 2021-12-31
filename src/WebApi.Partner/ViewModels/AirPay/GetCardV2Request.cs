using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.ViewModels.AirPay
{
    public class GetCardV2Request: POSGetCardV2Request
    {
        public string signature { get; set; }
        public string partner_id { get; set; }
    }
    public class POSGetCardV2Request
    {
        [Required]
        public string reference_no { get; set; }       
    }
}
