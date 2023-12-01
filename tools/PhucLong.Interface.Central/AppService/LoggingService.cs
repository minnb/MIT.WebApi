using Dapper;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.Database;
using RestSharp;
using System;
using System.Data;
using System.Net;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Central.AppService
{
    public class LoggingService
    {
        private IConfiguration _config;
        private DapperCentral _dapperContext;
        public LoggingService
            (
                IConfiguration config
            )
        {
            _config = config;
            _dapperContext = new DapperCentral(_config);
        }

        public void LoggingToDB(string jobName, string procedure, string description, string updateFlg = "N")
        {
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                var logging = new Logging()
                {
                    JobName = jobName,
                    StoreProcedure = procedure,
                    Message = description,
                    UpdateFlg = updateFlg,
                    CrtDate = DateTime.Now,
                    Id = Guid.NewGuid()
                };

                string queryIns = @"INSERT INTO dbo.Logs(JobName, StoreProcedure, Message, CrtDate, Id, UpdateFlg)
                                        VALUES (@JobName, @StoreProcedure, @Message, @CrtDate, @Id, @UpdateFlg)";

                conn.Execute(queryIns, logging, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ExceptionHelper.WriteExptionError("LoggingToDBAsync ", ex);
            }
        }

        public bool LoggingElastic(string url, LoggingElastic loggingElastic)
        {
            IRestResponse response = new RestResponse();
            try
            {
                RestClient client = new RestClient(url)
                {
                    Timeout = 30000
                };

                RestRequest restRequest = new RestRequest("api/common/logging", Method.POST);
                restRequest.AddHeader("Accept", "application/json");

                if (loggingElastic != null)
                {
                    restRequest.AddJsonBody(loggingElastic);
                }

                response = client.Execute(restRequest);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    FileHelper.WriteLogs("===> LoggingElastic: " + response.Content.ToString());
                    return true;
                }
                else
                {
                    FileHelper.WriteLogs("===> CallApiGCP.ERROR: " + response.Content.ToString());
                    return false;
                }

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> CallApiGCP.Exception: " + ex.Message.ToString());
                return false;
            }
        }
    }
}
