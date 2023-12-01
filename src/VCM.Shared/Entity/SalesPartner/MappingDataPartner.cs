using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.SalesPartner
{
    public class MappingDataPartner
    {
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public bool IsSync { get; set; }
        public string Description { get; set; }
        public int PictureId { get; set; }
        public int RefId { get; set; }
        public string Action { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime ChgeDate { get; set; }
        public Guid Id { get; set; }
    }
    public class ListMappingDataPartner: MappingDataPartner
    {
        public int StoreId { get; set; }
    }
}
