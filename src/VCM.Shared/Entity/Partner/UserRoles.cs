using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Partner
{
    [Table("Sys_User_Roles")]
    public class UserRoles
    {
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public bool Blocked { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
