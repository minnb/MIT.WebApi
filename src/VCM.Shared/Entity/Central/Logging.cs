using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    [Table("Logs")]
    public class Logging
    {
        public Guid Id { get; set; }
        public string JobName { get; set; }
        public string StoreProcedure { get; set; }
        public string Message { get; set; }
        public string UpdateFlg { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
