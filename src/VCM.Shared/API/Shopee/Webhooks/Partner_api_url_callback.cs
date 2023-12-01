using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Webhooks
{
    public class Partner_api_url_callback
    {
        public List<object_changes> object_changes { get; set; }
    }
    public class object_changes
    {
        public long event_id { get; set; }
        public string object_type { get; set; }
        public int object_id { get; set; }
        public string update_type { get; set; }
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        public string partner_object_id { get; set; }
    }
}
