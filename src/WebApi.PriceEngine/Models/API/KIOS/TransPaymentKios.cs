using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VCM.Shared.Dtos.PhucLong;

namespace WebApi.PriceEngine.Models.API.KIOS
{
    public class TransPaymentKios
    {
        [Required]
        [StringLength(20)]
        public string OrderNo { get; set; }
        [Required]
        [StringLength(8)]
        public string PaymentDate { get; set; }
        public List<OrderLoyalty> Loyalty { get; set; }
        public List<OrderPaymentDto> Payments { get; set; }
    }

}
