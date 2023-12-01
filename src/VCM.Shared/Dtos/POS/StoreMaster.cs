using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.POS
{
    public class StoreMaster
    {
        public string StoreNo { get; set; }
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string LocationCode { get; set; }
        public string Channel { get; set; }
        public string BranchNo { get; set; }
        public string BusinessAreaNo { get; set; }
    }
}
