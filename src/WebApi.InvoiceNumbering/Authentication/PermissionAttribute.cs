﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using VCM.Common.Helpers;
using VCM.Shared.Enums;

namespace WebApi.DrWin.Authentication
{
    //public class PermissionAttribute : TypeFilterAttribute
    //{
    //    public PermissionAttribute(PermissionEnum[] item) : base(typeof(AuthorizeActionFilter))
    //    {
    //        Arguments = new object[] { item };
    //    }
    //    public class AuthorizeActionFilter : IAuthorizationFilter
    //    {
    //        private readonly PermissionEnum[] _item;
    //        private readonly IRedisCacheService _memoryCacheService;
    //        public AuthorizeActionFilter(PermissionEnum[] item, IRedisCacheService memoryCacheService)
    //        {
    //            _item = item;
    //            _memoryCacheService = memoryCacheService;
    //        }
    //        public void OnAuthorization(AuthorizationFilterContext context)
    //        {
    //            string request = context.HttpContext.Request?.Headers["Authorization"].ToString();

    //            string userName = ConvertHelper.GetUserNameAuthorization(request);
    //            if (!string.IsNullOrEmpty(userName))
    //            {
    //                List<string> roles = new List<string>();
    //                foreach (PermissionEnum item in _item)
    //                {
    //                    roles.Add(item.ToString());
    //                }
    //                var userRoles = _memoryCacheService.GetUserRolesAsync().Result;
    //                bool isUserPermission = userRoles.Where(x => roles.Contains(x.RoleName) && x.UserName == userName).Any();
    //                if (!isUserPermission)
    //                {
    //                    context.Result = new JsonResult(ResponseHelper.RspNotHaveAccess());
    //                    return;
    //                }
    //            }
    //            else
    //            {
    //                return;
    //            }
    //        }
    //    }
    //}
}