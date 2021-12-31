using VCM.Shared.Entity.Partner;
using VCM.Shared.SeedWork;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace VCM.Partner.API.ViewModels.Partner
{
    public class GetStoreKiosPaging: PagedList<StoreAndKios>
    {
        public string PosOdoo { get; set; }
        public string StoreNo { get; set; }
        public string SearchKeyword { get; set; }
        [Required]
        [DefaultValue(1)]
        public int PageIndex { get; set; } = 1;
        [Required]
        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;
    }
}
