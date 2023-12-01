using Microsoft.AspNetCore.Authorization;
using VCM.Shared.Enums;

namespace WebApi.DrWin.Authentication
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
