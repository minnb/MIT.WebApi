using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Odoo.Database
{
    public class ConndbCentral
    {
        private readonly IConfiguration _configuration;
        public ConndbCentral(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection ConnCentralPhucLong
        {
            get
            {
                return new SqlConnection(_configuration["ConnectionStrings:PhucLongStaging"]);
            }
        }

        public List<InterfaceEntry> GetInterfaceEntry(IDbConnection conn, string jobType)
        {
            string query = @"SELECT [AppCode],[JobName],[JobType],[SortOrder],[LocalPath],[LocalPathArchived],[SftpPath],[SftpPathArchived],[SftpHost],[SftpUser],[SftpPass]
                            ,[StoreProName],[MaxFile],[SfptPermissions],[FileExtension],[SftpOS],[SftpPort],[IsMoveFile],[Blocked],[CrtDate],[Prefix]  
                            FROM [dbo].[InterfaceEntry] (NOLOCK) WHERE [Blocked] = 0 AND [JobType] = '" + jobType+ "' ORDER BY SortOrder;";

            return conn.Query<InterfaceEntry>(query).ToList();
        }
    }
}
