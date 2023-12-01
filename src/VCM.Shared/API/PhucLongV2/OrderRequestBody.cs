using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Dtos.PhucLong;

namespace VCM.Shared.API.PhucLongV2
{ 
    public class OrderRequestBody : OrderDto
    {
        public List<OrderLineDto> Items { get; set; }
        public List<OrderPaymentDto> Payments { get; set; }
        public List<MembershipCard> MembershipCard { get; set; }
    }
    public class OrderResponseBody : RspOrderDto
    {
        public List<RspOrderLineDto> Items { get; set; }
        public List<OrderPaymentDto> Payments { get; set; }
        public List<MembershipCard> MembershipCard { get; set; }
    }

}
