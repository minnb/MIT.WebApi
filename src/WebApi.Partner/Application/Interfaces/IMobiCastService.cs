using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Shared.API;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.MBC;
using VCM.Shared.Entity.Partner;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IMobiCastService
    {
        Task<ResponseClient> GetListBillMBC_Triple(RequestListOrderPOS request, WebApiViewModel webApiInfo, UserMBC userMBC, string proxyHttp, string[] byPass);
        Task<ResponseClient> GetBillDetailMBC_Triple(RequestTransaction request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> GetSerialMBC_Triple(RequestTransaction request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> UpdateStatusOrderMBC_Triple(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, UserMBC userMBC, string proxyHttp, string[] byPass);
        Task<ResponseClient> CancelOrderMBC_TripleAsync(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, UserMBC userMBC, string proxyHttp, string[] byPass);
        Task<ResponseClient> PurchaseCardMBC_TripleAsync(POSPurchaseCardRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> TopupMBC_Triple(POSTopupRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
    }
}
