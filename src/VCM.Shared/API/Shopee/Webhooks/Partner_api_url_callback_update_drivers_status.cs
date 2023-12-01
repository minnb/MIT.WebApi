using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.Shopee.Webhooks
{
    public class partner_api_url_callback_update_drivers_status
    {
        public List<driver_arriving_times> driver_arriving_times { get; set; }
    }
    public class driver_arriving_times
    {
        public string order_code { get; set; }
        public int restaurant_id { get; set; }
        public string partner_restaurant_id { get; set; }
        public int driver_uid { get; set; }
        public int arriving_time { get; set; }
    }
}
