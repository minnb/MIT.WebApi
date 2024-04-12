using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.API.Shopee.Webhooks;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.Partner.Shopee;
using VCM.Shared.Entity.SalesPartner;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IShopeeService
    {
        Task<ResponseClient> GetOrderList(RequestListOrderPOS request, WebApiViewModel webApi, string proxy, string[] bypass);
        Task<ResponseClient> GetOrderDetail(RequestTransaction request, WebApiViewModel webApi, List<Item> itemDto, string proxy, string[] bypass);
        ResponseClient UpdateStatusOrder(RequestUpdateOrderStatus request, WebApiViewModel webApi, string proxy, string[] bypass);
        List<ItemSalesOnApp> GetShopeeDishesCache(bool isDelete = false);
        Task<bool> Shopee_update_order(Partner_api_url_callback_update_order request);
        Task<bool> Shopee_export_error_menu(export_error_menu request);
        Task<bool> Shopee_update_drivers_status(partner_api_url_callback_update_drivers_status request);
        Task<bool> Shopee_update_menu(Partner_api_url_callback request);
        Task<ResponseClient> CountedOrder(CountOrderRequest request, WebApiViewModel webApi);

    }
}
