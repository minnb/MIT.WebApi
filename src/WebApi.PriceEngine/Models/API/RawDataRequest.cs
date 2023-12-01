using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VCM.Shared.Const;

namespace WebApi.PriceEngine.Models.API
{
    public class RawDataRequest
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        [DefaultValue("WCM")]
        public string AppCode { get; set; } = "WCM";
        public string PartnerCode
        {
            get
            {
                return AppPartnerPLHConst.GetPartnerCode(AppCode);
            }
        }
        [Required]
        [StringLength(6, MinimumLength = 4)]
        [DefaultValue("2018")]
        public string StoreNo { get; set; } = "2018";
        [Required]
        [StringLength(20)]
        public string OrderNo { get; set; }
        [Required]
        public string JsonData { get; set; }
        [Required]
        [StringLength(50)]
        public string RequestId { get; set; }
    }
}
