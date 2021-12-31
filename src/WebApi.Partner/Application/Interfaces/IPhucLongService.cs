using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Shared.API;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.Entity.Partner;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IPhucLongService
    {
        Task<ResponseClient> GetOrderListPhucLong(RequestListOrderPOS request, WebApiViewModel webApiInfo);
        Task<ResponseClient> GetOrderDetailPhucLong(RequestTransaction request, WebApiViewModel webApiInfo, List<Item> itemDto);
        Task<ResponseClient> UpdateStatusOrderPhucLong(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo);
        Task<ResponseClient> MappingStoreAndKios(WebApiViewModel webApiInfo, bool IsMapping, string StoreNo, string PosOdoo);
        Task<ResponseClient> UpdateStatusVoucherPhucLong(RequestVoucher request, WebApiViewModel webApiInfo);

    }
}
