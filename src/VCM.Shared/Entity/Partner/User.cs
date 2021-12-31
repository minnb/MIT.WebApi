using System.ComponentModel.DataAnnotations.Schema;
using VCM.Shared.Partner;

namespace VCM.Shared.Entity.Partner
{
    [Table("Sys_User")]
    public class User: InfoDto
    {
        public int  Id { get; set; }
        public string AppCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool Blocked { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsLockoutEnabled { get; set; }
        public bool IsPhoneConfirmed { get; set; }
    }
}
