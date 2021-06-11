using MIT.WebApi.GPAY.ViewModels;
using MIT.WebApi.GPAY.ViewModels.AirPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.WebApi.GPAY.Application.Interfaces
{
    public interface IAirPayService
    {
        public ResponseObject CallPurchaseCardV2(WebApiViewModel _webApiAirPayInfo, POSPurchaseCardRequest bodyData);
    }
}
