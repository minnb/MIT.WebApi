using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.ViewModels.WMT
{
    public class RqListOrderWCM
    {
        public string OrderCode { get; set; }
        public string StoreId   { get; set; }
        public string ChannelId { get; set; }
        public string ChainId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool OrderByDesc { get; set; }
        public string OrderBy { get; set; }

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
