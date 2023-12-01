using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.API;
using VCM.Shared.API.O2;
using VCM.Shared.API.Wintel;
using VCM.Shared.Entity.Partner;

namespace WebApi.Partner.Application.Interfaces
{
    public interface IO2Service
    {
        Task<ResponseClient> LoginO2Async(WebApiViewModel webApiInfo);
        Task<ResponseClient> CheckPhoneNumberAsync(WebApiViewModel webApiInfo, ValidateKitStatusRequest request, List<Item> itemDto);
        Task<ResponseClient> UpdateTierMemberO2Async(WebApiViewModel webApiInfo, UpdateTierMemberO2 request);
        Task<ResponseClient> PingO2(WebApiViewModel webApiInfo);
    }
}
