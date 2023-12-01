using System.Collections.Generic;
using VCM.Shared.Entity.Partner;

namespace WebApi.PhucLong.Models
{
    public class WebApiModel : SysWebApi
    {
        public IList<SysWebRoute> WebRoute { get; set; }
    }
}
