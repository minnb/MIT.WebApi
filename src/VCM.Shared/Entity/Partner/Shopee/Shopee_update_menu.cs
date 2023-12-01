using System.ComponentModel.DataAnnotations.Schema;
using VCM.Shared.API.Shopee.Webhooks;

namespace VCM.Shared.Entity.Partner.Shopee
{
    [Table("Shopee_update_menu")]
    public class Shopee_update_menu : object_changes
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}
