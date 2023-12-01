using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    [Table("Jobs")]
    public class Jobs
    {
        public int Id { get; set; }
        public int Sort { get; set; }
        public string JobType { get; set; }
        public string JobName { get; set; }
        public string Procedure { get; set; }
        public bool Blocked { get; set; }
    }
}
