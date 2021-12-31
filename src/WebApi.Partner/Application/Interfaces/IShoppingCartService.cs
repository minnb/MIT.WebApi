using System.Threading.Tasks;
using VCM.Shared.API;
using VCM.Partner.API.ViewModels.AirPay;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IShoppingCartService
    {
        Task<ResponseClient> GetListBill_WMTAsync(RequestListOrderPOS request, WebApiViewModel webApiInfo,  string proxyHttp, string[] byPass);
        Task<ResponseClient> GetBillDetail_WMTAsync(RequestTransaction request, WebApiViewModel webApiInfo,  string proxyHttp, string[] byPass);
        Task<ResponseClient> UpdateStatusOrder_WMTAsync(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass);
    }
}
