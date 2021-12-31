using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Entity.Partner
{
    [Table("Sys_WebRoute")]
	public class SysWebRoute
	{
		public int Id { get; set; }
		public string AppCode { get; set; }
		public string Name { get; set; }
		public string Route { get; set; }
		public string Description { get; set; }
		public bool Blocked { get; set; }
		public string Version { get; set; }
		public string Notes { get; set; }
	}
}
