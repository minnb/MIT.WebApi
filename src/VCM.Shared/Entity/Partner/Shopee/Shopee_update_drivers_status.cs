using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VCM.Shared.API.Shopee.Webhooks;

namespace VCM.Shared.Entity.Partner.Shopee
{
    [Table("Shopee_update_drivers_status")]
    public class Shopee_update_drivers_status: driver_arriving_times
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}
