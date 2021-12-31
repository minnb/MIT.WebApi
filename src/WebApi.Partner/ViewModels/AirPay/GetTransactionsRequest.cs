using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.ViewModels.AirPay
{
    public class GetTransactionsRequest
    {
        [Required]
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        public DateTime from_date { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        public DateTime to_date { get; set; }
    }
    public class GetTransaction
    {
        public string to_date { get; set; }
        public string from_date { get; set; }
        public string partner_id { get; set; }
        public string signature { get; set; }
    }
}
