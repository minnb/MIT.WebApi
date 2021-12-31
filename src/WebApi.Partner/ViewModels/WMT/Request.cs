using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.ViewModels.WMT
{
    public class RqListOrderWMT
    {
        public string OrderNo { get; set; }
        public string[] Status { get; set; }
        public string StoreId   { get; set; }
        public string ChannelId { get; set; }

    }
    public class RqDetailOrderWMT
    {
        public string[] Status { get; set; }
        public string StoreId { get; set; }
        public string ChannelId { get; set; }

    }

    public class RqUpdateStatusOrderWMT
    {
        public string Status { get; set; }
    }
}
