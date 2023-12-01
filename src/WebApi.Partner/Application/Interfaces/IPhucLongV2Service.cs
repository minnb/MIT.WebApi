using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Entity.Partner;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IPhucLongV2Service
    {
        Task<ResponseClient> CreateOrderAsync(OrderRequestBody orderRequestBody);
        Task<ResponseClient> GetOrderList(RequestListOrderPOS request);
        Task<ResponseClient> GetOrderDetail(RequestTransaction request, List<Item> itemDto);
        Task<ResponseClient> UpdateStatusOrder(RequestUpdateOrderStatus request, WebApiViewModel webApi, string proxy, string[] bypass);
        Task<ResponseClient> CountedOrder(CountOrderRequest request, WebApiViewModel webApi);
    }
}
