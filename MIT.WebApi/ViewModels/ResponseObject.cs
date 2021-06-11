using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.WebApi.GPAY.ViewModels
{
    public class ResponseObject
    {
        public Meta Meta { get; set; }
        public object Data { get; set; }
    }
    public class Meta
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
