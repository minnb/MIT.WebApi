using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Dtos.PhucLong;

namespace VCM.Shared.API.MakeOrder
{
    public class OrderRequestBody : OrderDto
    {
        public List<OrderLineDto> Items { get; set; }
        public List<OrderPaymentDto> Payments { get; set; }
    }
}
