using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.Shopee.Webhooks
{
    public class Partner_api_url_callback_update_order
    {
        [Required]
        public string order_code { get; set; }
        public int update_type { get; set; }
        public int restaurant_id { get; set; }
        public string pick_time { get; set; }
        public int status { get; set; }
        public string merchant_note { get; set; }
        public string note_for_shipper { get; set; }
        public string partner_restaurant_id { get; set; }
        public string serial { get; set; }
    }
}
