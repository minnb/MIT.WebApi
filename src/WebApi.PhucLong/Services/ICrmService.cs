using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.API.PLG;
using VCM.Shared.Entity.PhucLong.Dtos;

namespace WebApi.PhucLong.Services
{
    public interface ICrmService
    {
        Task<ResponseVoucherCheckOdoo> CheckVoucherOdoo(RequestVoucherCheckOdoo request);
        Task<ResponseVoucherRedeemOdoo> VoucherRedeemOdoo(RequestVoucherRedeemOdoo request);
        Task<VoucherInfoDto> UpdateStatusVoucher(RequestVoucherPLG request);
    }
}
