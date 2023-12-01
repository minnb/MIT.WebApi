using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Entity.Partner;

namespace VCM.Shared.Dtos
{
    public class SysWebApiDto : SysWebApi
    {
        public IList<SysWebRoute> WebRoute { get; set; }
    }
}
