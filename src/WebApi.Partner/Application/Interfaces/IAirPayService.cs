
using System.Collections.Generic;
using VCM.Shared.API;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.Entity.Partner;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IAirPayService
    {
        public ResponseClient CallPurchaseCardV2(WebApiViewModel _webApiAirPayInfo, POSPurchaseCardRequest bodyData, List<Item> itemDto, string privateKeyVCM, string proxyHttp, string[] byPass);
        public ResponseClient GetCardV2(WebApiViewModel _webApiAirPayInfo, POSGetCardV2Request bodyData, string privateKeyVCM, string proxyHttp, string[] byPass);
        public ResponseClient GetBalance(WebApiViewModel _webApiAirPayInfo, string privateKeyVCM, string proxyHttp, string[] byPass);
        public ResponseClient GetTransactions(WebApiViewModel _webApiAirPayInfo, GetTransactionsRequest bodyData, string privateKeyVCM, string proxyHttp, string[] byPass);
    }
}
