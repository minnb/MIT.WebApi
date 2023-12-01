using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Shared.API;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.MBC;
using VCM.Shared.Entity.Partner;
using VCM.Shared.API.Wintel;
using WebApi.Partner.ViewModels.MBC;

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
        Task<ResponseClient> ValidateKitStatus_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> UpdateKitStatusKYC_MBC_Triple(RequestUpdateOrderStatus request, TripleObj wsRequest, string serialNo,  WebApiViewModel webApiInfo, UserMBC userMBC, string proxyHttp, string[] byPass);
        Task<ResponseClient> CheckExtendSubscriberInfo_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);

        Task<ResponseClient> GetSingleIsdn_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> KeepIsdn_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> CreateEsimOrder_MBC_Triple(CreateEsimOrderPosWcmRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> GetListPackage_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);

        //Sim vật lý
        Task<ResponseClient> GetListPackagePhysicalSim_Wintel_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> GetSingleIsdnNoneEsim_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> ValidateSimStatu_MBC_Triple(CreatePhysicalSimOrderPosWcmRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
        Task<ResponseClient> CreatePhysicalSimOrder_MBC_Triple(CreatePhysicalSimOrderPosWcmRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass);
    }
}
