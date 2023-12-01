using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Entity.Partner;

namespace Tools.Interface.Models
{
    public class WebApiInfo : SysWebApi
    {
        public IList<SysWebRoute> WebRoute { get; set; }
    }
}
