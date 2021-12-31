using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.API;
using VCM.Shared.API.MakeOrder;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IMakeOrderService
    {
        Task<ResponseClient> CreateOrderAsync(OrderRequestBody orderRequestBody);
    }
}
