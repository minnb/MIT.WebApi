using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.API;
using VCM.Shared.Dtos.WinMoney;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IWinMoneyService
    {
        ResponseClient RspURL_WMC(WebApiViewModel webApiInfo, POSRequestUrl_WMC request);
        List<StoreInfoWMC> GetStoreInfo(string _connectCentralMD);
        List<CashierInfoWMC> GetCashierInfo(string _connectCentralMD, string storeNo);
        List<PosInfoWMC> GetPosInfo(string _connectCentralMD, string storeNo);
    }
}
