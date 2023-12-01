using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VCM.Shared.API.Shopee.Webhooks;

namespace VCM.Shared.Entity.Partner.Shopee
{
    [Table("Shopee_export_error_menu")]
    public class Shopee_export_error_menu: export_error_menu
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}
