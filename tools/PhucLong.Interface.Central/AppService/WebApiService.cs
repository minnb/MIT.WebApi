using Dapper;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using VCM.Shared.Entity.Partner;

namespace PhucLong.Interface.Central.AppService
{
    public class WebApiService
    {
        private IConfiguration _configuration;
        public WebApiService
           (
               IConfiguration config
           )
        {
            _configuration = config;
        }

        public WebApiModel GetWebApiInfo(IDbConnection conn, string appCode)
        {
            try
            {
                var api = conn.Query<SysWebApi>(@"SELECT [Id],[AppCode],[Host],[Description],[UserName],[Password],[PublicKey],[PrivateKey],[Blocked],[Version]  
                                                        FROM [dbo].[WebApi] (NOLOCK)
                                                        WHERE [Blocked] = 0 AND AppCode = '" + appCode.ToUpper()  + @"'  ORDER BY Id;").ToList().FirstOrDefault();

                var routeApi = conn.Query<SysWebRoute>(@"SELECT [Id],[AppCode],[Name],[Route],[Description],[Blocked],[Version],[Notes]  FROM [dbo].[WebRoute] (NOLOCK) WHERE AppCode = '" + appCode.ToUpper() + @"';").ToList();

                if (api != null)
                {
                    return new WebApiModel()
                    {
                        Id = api.Id,
                        Host = api.Host,
                        AppCode = api.AppCode,
                        UserName = api.UserName,
                        Password = api.Password,
                        PublicKey = api.PublicKey,
                        PrivateKey = api.PrivateKey,
                        Blocked = api.Blocked,
                        Description = api.Description,
                        WebRoute = routeApi?.Where(x => x.AppCode == api.AppCode).ToList()
                    };
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
    public class WebApiModel : SysWebApi
    {
        public List<SysWebRoute> WebRoute { get; set; }
    }
}
