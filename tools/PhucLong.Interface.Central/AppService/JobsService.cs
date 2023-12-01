using Dapper;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using VCM.Common.Helpers;

namespace PhucLong.Interface.Central.AppService
{
    public class JobsService
    {
        private IConfiguration _configuration;
        private DapperCentral _dapperContext;

        public JobsService
          (
              IConfiguration config
          )
        {
            _configuration = config;
            _dapperContext = new DapperCentral(_configuration);
        }

        public async System.Threading.Tasks.Task RunJobsAsync()
        {
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
            conn.Open();
            try
            {
                var jobs = _dapperContext.GetJobs(conn, "STORE"); 
                if(jobs.Count > 0)
                {
                    foreach(var item in jobs)
                    {
                        if (!string.IsNullOrEmpty(item.Procedure))
                        {
                            FileHelper.WriteLogs(item.JobName.ToString() + " ===> START " + item.Procedure);
                            await conn.QueryAsync(@"EXEC " + item.Procedure, commandTimeout: 36000);
                            FileHelper.WriteLogs("===> END " + item.Procedure);
                            Thread.Sleep(500);
                        }
                    }
                }
                else
                {
                    FileHelper.WriteLogs("... nothing run");
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("RunJobs Exception:" + ex.Message.ToString());
            }
        }
    }
}
