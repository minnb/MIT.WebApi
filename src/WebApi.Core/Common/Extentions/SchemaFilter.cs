using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Core.Common.Extentions
{
    public class SchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null)
            {
                return;
            }

            foreach (var property in schema.Properties)
            {
                if (property.Value.Default != null && property.Value.Example == null)
                {
                    property.Value.Example = property.Value.Default;
                }
            }
        }

        //private string ToCamelCase(string name)
        //{
        //    return char.ToLowerInvariant(name[0]) + name.Substring(1);
        //}
    }
}
