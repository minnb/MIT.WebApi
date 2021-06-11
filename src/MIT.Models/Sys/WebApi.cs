using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Dtos
{
	[Table("Sys_WebApi")]
	public class WebApiDto
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

	}

	[Table("Sys_WebRoute")]
	public class WebRouteDto
	{
		public int Id { get; set; }
		public string AppCode { get; set; }
		public string Name { get; set; }
		public string Route { get; set; }
		public string Description { get; set; }
		public bool Blocked { get; set; }
	}
}