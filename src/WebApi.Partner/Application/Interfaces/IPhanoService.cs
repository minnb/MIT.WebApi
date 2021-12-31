using System.Collections.Generic;
using VCM.Shared.API;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.Entity.Partner;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IPhanoService
    {
        //Phano
        ResponseClient GetSalesOrderPhano(RequestTransaction request, WebApiViewModel webApiInfo, List<Item> item, string proxyHttp, string[] byPass);
        ResponseClient UpdateSalesOrderPhano(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass);
    }
}
