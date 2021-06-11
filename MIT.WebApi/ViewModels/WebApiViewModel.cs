using MIT.Dtos;
using System.Collections.Generic;

namespace MIT.WebApi.GPAY.ViewModels.AirPay
{
    public class WebApiViewModel: WebApiDto
    {
        public IList<WebRouteDto> WebRouteDto { get; set; }
    }
}
