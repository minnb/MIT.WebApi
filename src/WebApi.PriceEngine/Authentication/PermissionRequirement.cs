using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.Enums;

namespace WebApiPriceEngine.Authentication
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(PermissionEnum permission)
        {
            Permission = permission;
        }

        public PermissionEnum Permission { get; }
    }
}
