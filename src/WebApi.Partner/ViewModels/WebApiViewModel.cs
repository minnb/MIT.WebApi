using System.Collections.Generic;
using VCM.Shared.Entity.Partner;

namespace VCM.Partner.API.ViewModels.AirPay
{
    public class WebApiViewModel: SysWebApi
    {
        public IList<SysWebRoute> WebRoute { get; set; }
    }
}
