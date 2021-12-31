using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Entity.Central
{
    [Table("InterfaceSetup")]
    public class InterfaceSetup
    {
        public string AppCode { get; set; }
        public string JobName { get; set; }
        public string Scheduler { get; set; }
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public string Hour { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string Prefix_File { get; set; }
        public string Blocked { get; set; }
        public string CrtDate { get; set; }
    }
}
