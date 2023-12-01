using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API
{
    public class LoggingElastic
    {
        public string RequestId { get; set; }
        public string IpAddress { get; set; }
        public string JobName { get; set; }
        public long RunTime { get; set; }
        public string DeveloperMessage { get; set; }
    }
}
