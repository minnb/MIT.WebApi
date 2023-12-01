using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Shared.API;
using VCM.Shared.Dtos.PhucLong;
using WebApi.PriceEngine.Models.API.KIOS;

namespace WebApi.PriceEngine.Application.Interfaces
{
    public interface IBillPaymentService
    {
        Task<ResponseClient> OrderDetailKIOS(string UserName, string AppCode, string OrderNo);
        Task<ResponseClient> UpdatePaymentKIOS(string UserName, string AppCode, TransPaymentKios Payments);
    }
}
