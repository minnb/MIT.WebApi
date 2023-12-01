using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Webhooks
{
    public class export_error_menu
    {
        public string object_type { get; set; }
        public int object_id { get; set; }
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        public string partner_object_id { get; set; }
        public long event_id { get; set; }
        public string update_type { get; set; }
        public string error_type { get; set; }
        public string error_time { get; set; }

    }
}
