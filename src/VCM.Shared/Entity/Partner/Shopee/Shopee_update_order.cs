using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VCM.Shared.API.Shopee.Webhooks;

namespace VCM.Shared.Entity.Partner.Shopee
{
    [Table("Shopee_update_order")]
    public class Shopee_update_order : Partner_api_url_callback_update_order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string update_flg { get; set; }
        public string message { get; set; }
        public string crt_user { get; set; }
        public DateTime chg_date { get; set; }
        public string raw_data { get; set; }
    }
}
