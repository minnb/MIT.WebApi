using System.Threading.Tasks;
using VCM.Shared.API;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.API.PhucLongV2;
using WebApi.Partner.ViewModels.Partner;
using System.Collections.Generic;
using VCM.Shared.Entity.Partner;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IShoppingCartService
    {
        Task<ResponseClient> GetListBill_WCM_Async(RequestListOrderPOS request, WebApiViewModel webApiInfo,  string proxyHttp, string[] byPass, string requestId = "");
        Task<ResponseClient> GetBillDetail_WMT_Async(RequestTransaction request, WebApiViewModel webApiInfo,  string proxyHttp, string[] byPass, string requestId = "");
        Task<ResponseClient> GetBillDetail_WCM_Async(RequestTransaction request, WebApiViewModel webApiInfo, List<VAT> vATs, string proxyHttp, string[] byPass, string requestId = "");
        Task<ResponseClient> UpdateStatusOrder_WCM_Async(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass, string requestId = "");
        Task<ResponseClient> CountedOrder_WCM(CountOrderRequest request, WebApiViewModel webApi, string requestId = "");
        Task<ResponseClient> RefundDetails_WebOnline(SalesReturnRequest request, WebApiViewModel webApi, string requestId = "");
    }
}
