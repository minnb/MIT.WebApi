using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Partner
{
    [Table("M_PartnerMapping")]
    public class PartnerMapping : PartnerMappingDto
    {
    }
    public class PartnerMappingDto
    {
        public string PartnerCode { get; set; }
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public DateTime CrtDate { get; set; }
        public bool Blocked { get; set; }

    }
}
