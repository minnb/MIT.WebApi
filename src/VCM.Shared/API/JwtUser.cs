using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API
{
    public class JwtUser
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
    public class JwtData
    {
        public string UserName { get; set; }
        public string Expiration { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
    }
}
