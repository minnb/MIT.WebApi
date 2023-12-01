using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Partner
{
    [Table("Sys_Config")]
    public class SysConfig
    {
        public string AppCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Prefix { get; set; }
        public bool Blocked { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
