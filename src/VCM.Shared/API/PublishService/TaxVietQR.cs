using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.PublishService
{
    public class ResponseTaxVietQR
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public DataTaxVietQR Data { get; set; }
    }
    public class DataTaxVietQR
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
