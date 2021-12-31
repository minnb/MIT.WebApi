using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Shared.API;
using VCM.Shared.API.PLG;
using VCM.Shared.Entity.PhucLong;

namespace VCM.PhucLong.API.Services
{
    public interface ITransactionService
    {
        Task<List<ResponseOrderList>> GetOrderListAsync(RequestOrderList request);
        Task<ResponseOrderDetail> GetOrderAsync(int set, string order_no, int location_id);
        bool UpdateStatusOrderAsync(int set, string order_no, int status);
        Task<Pos_Staging> GetOrderByIdAsync(int set, string order_id, int location_id);
    }
}
