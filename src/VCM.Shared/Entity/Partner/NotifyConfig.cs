using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Partner
{
    [Table("M_NotifyConfig")]
    public class NotifyConfig
    {
        public string AppCode { get; set; }
        public string ActionType { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string MessageFormat { get; set; }
        public bool Blocked { get; set; }
        public bool IsOffline { get; set; }
    }
}
