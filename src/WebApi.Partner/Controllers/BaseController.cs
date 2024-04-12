using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using VCM.Shared.API;

namespace VCM.Partner.API.Controllers
{
    
    [Authorize]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {
            
        }
        protected JwtData GetJwtData()
        {
            return new JwtData()
            {
                UserName = User.Identity.Name,
                Expiration = User.FindFirst("exp")?.Value,
                Issuer = User.FindFirst("iss")?.Value,
                Audience = User.FindFirst("aud")?.Value
            };
        }
        public static string LogInvalidModelState(ModelStateDictionary modelState)
        {
            var modelStateEntries = new List<ModelStateEntry>();
            var errorMessages = new List<string>();

            Visit(modelState.Root, modelStateEntries);

            foreach (var modelStateEntry in modelStateEntries)
            {
                foreach (var error in modelStateEntry.Errors)
                    errorMessages.Add(error.ErrorMessage);
            }

            return string.Format("Invalid Model State: {0}", errorMessages[0]);
        }
        private static void Visit(ModelStateEntry modelStateEntry, ICollection<ModelStateEntry> modelStateEntries)
        {
            if (modelStateEntry.Children != null)
            {
                foreach (var child in modelStateEntry.Children)
                    Visit(child, modelStateEntries);
            }

            if (!modelStateEntry.IsContainerNode)
                modelStateEntries.Add(modelStateEntry);
        }
    }
}
