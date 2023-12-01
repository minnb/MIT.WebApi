using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Entity.Partner
{
    [Table("Sys_WebApi")]
	public class SysWebApi
    {
		public int Id { get; set; }
		public string AppCode { get; set; }
		public string Host { get; set; }
		public string Description { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string PublicKey { get; set; }
		public string PrivateKey { get; set; }
		public bool Blocked { get; set; }
		public string HttpProxy { get; set; }
		public string Bypasslist { get; set; }
	}
}