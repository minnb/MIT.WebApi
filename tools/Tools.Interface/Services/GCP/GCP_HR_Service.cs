using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using VCM.Common.Database;
using VCM.Common.Helpers;

namespace Tools.Interface.Services.GCP
{
    public class GCP_HR_Service
    {
        private DapperContext _dapperContext;
        public GCP_HR_Service()
        {
        }

        public void Exp_FromTable_MSN(string appCode, string connectionString, string jobType, string pathLocal)
        {
            //SELECT * FROM [dbo].[MSN_HR]
            CreateFile_FromTable_MSN(appCode, connectionString, jobType, pathLocal, "MSN_HR", Query_MSN_HR());
            //SELECT* FROM[dbo].[MSN_Recruitment]
            CreateFile_FromTable_MSN(appCode, connectionString, jobType, pathLocal, "MSN_Recruitment", Query_MSN_Recruitment());
            //SELECT* FROM[dbo].[MSN_RevenueProfit]
            CreateFile_FromTable_MSN(appCode, connectionString, jobType, pathLocal, "MSN_RevenueProfit", Query_MSN_RevenueProfit());
            //SELECT* FROM[dbo].[MSN_Training]
            CreateFile_FromTable_MSN(appCode, connectionString, jobType, pathLocal, "MSN_Training", Query_MSN_Training());
            //SELECT* FROM[dbo].[MSN_Turnover]
            CreateFile_FromTable_MSN(appCode, connectionString, jobType, pathLocal, "MSN_Turnover", Query_MSN_Turnover());
        }

        private bool CreateFile_FromTable_MSN(string appCode, string connectionString, string jobType, string pathLocal,string TableName, string sQuery)
        {
            bool flg = true;
            _dapperContext = new DapperContext(connectionString);
            using IDbConnection conn = _dapperContext.CreateConnDB;
            conn.Open();
            try
            {
                Console.WriteLine("processing {0}", TableName);
                FileHelper.Write2Logs(jobType, TableName);
                var data_HR = conn.Query(sQuery, commandTimeout: 36000).ToList();
                if (data_HR.Count > 0)
                {
                    CreateFileHCM(appCode, jobType, TableName, pathLocal, conn, data_HR);
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(jobType, TableName + " Exception" + ex.Message.ToString());
                flg = false;
            }
            return flg;
        }  
        private bool CreateFileHCM(string appCode, string jobType, string TableName, string pathLocal, IDbConnection conn, List<dynamic> data_HR)
        {
            try
            {
                List<MSN_Temp> MSN_Temps = new List<MSN_Temp>();
                FileHelper.Write2Logs(jobType, TableName + ": " + data_HR.Count.ToString());
                if (FileHelper.CreateFileMaster(appCode, TableName, pathLocal, JsonConvert.SerializeObject(data_HR)))
                {
                    var getListID = data_HR.Select(x => new { x.Id }).ToList();
                    foreach (var hr in getListID)
                    {
                        MSN_Temps.Add(new MSN_Temp()
                        {
                            Id = hr.Id,
                            TableName = TableName
                        });
                    }
                    if (InsertMSNTemp(conn, MSN_Temps))
                    {
                        MSN_Temps.Clear();
                    }
                    FileHelper.Write2Logs(jobType, "Created file: " + TableName);
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(jobType, TableName + " CreateFileHCM Exception" + ex.Message.ToString());
                return false;
            }
        }
        private string Query_MSN_Turnover()
        {
            return @"EXEC SP_GET_MSN_Turnover;";
            //return @"SELECT [Id],[Year],[EmployeeID],[FirstName],[LastName],[FullName],[Gender],[DOB],[Age],[Entity],[Department],[Onboarddate],[Title],[Rank],[Function]
            //                ,[Stafftype],[Onboard],[OnboardOfficial],[EffectiveOff],[ContractType],[NumberOfWorkingYears],[NumberOfWorkingMonths],[LocalExpat],[TypeOfLeaving],[9box] AS F9box
            //                ,[DirectIndirect],[CountTurnover],[BU],[VoluntaryInvoluntary],[ProcessingMonth],[ProcessingYear]
            //         FROM " + TableName + " (NOLOCK) A "
            //         + " WHERE NOT EXISTS (SELECT 1 FROM MSN_Temp (NOLOCK) B WHERE A.Id = B.Id AND B.[TableName] = '" + TableName + "') ORDER BY A.Id";

        }
        private string Query_MSN_Training()
        {
            return @"EXEC SP_GET_MSN_Training;";
            //return @"SELECT [Id],[Year],[BU],[Entity],[Function],[EmployeeID],[FullName],[Rank],[Position],[LineManager],[HRBP],[CourseID],[Course],[Organizer]
            //            ,[Type],[Supplier],[Hour],[TrainingCost],[DirectIndirect],[ProcessingMonth],[ProcessingYear]
            //                      FROM " + TableName + " (NOLOCK) A "
            //                            + " WHERE NOT EXISTS (SELECT 1 FROM MSN_Temp (NOLOCK) B WHERE A.Id = B.Id AND B.[TableName] = '" + TableName + "') ORDER BY A.Id";

        }
        private string Query_MSN_RevenueProfit()
        {
            return @"EXEC SP_GET_MSN_RevenueProfit;";
            //return @"SELECT [Id],[Year],[BU],[Revenue],[Profit],[Hrcost],[ProcessingMonth],[ProcessingYear]
            //                      FROM " + TableName + " (NOLOCK) A "
            //                            + " WHERE NOT EXISTS (SELECT 1 FROM MSN_Temp (NOLOCK) B WHERE A.Id = B.Id AND B.[TableName] = '" + TableName + "') ORDER BY A.Id";

        }
        private string Query_MSN_Recruitment()
        {
            return @"EXEC SP_GET_MSN_Recruitment;";
            //return  @"SELECT [Id],[Year],[BU],[Entity],[DirectIndirect],[Function],[APNumber],[Department],[Title],[DateOfRequest],[ExpectedOnboardMonth],[DateOfReplace]
            //                            ,[Reason],[NumberOfHire],[Rank],[Status],[ReasonIfClosed],[RecruitmentSources],[SubRecruitmentSources],[ClosingDate],[OnboardingDate],[EmployeeCode]
            //                            ,[FullName],[TA],[HRBP],[Probation],[LeavingDateIfFailProbation],[ReasonLeaving],[ExitInterviewYesNo],[ProcessingMonth],[ProcessingYear]
            //                            FROM " + TableName + " (NOLOCK) A "
            //                            + " WHERE NOT EXISTS (SELECT 1 FROM MSN_Temp (NOLOCK) B WHERE A.Id = B.Id AND B.[TableName] = '" + TableName + "') ORDER BY A.Id";

        }
        private string Query_MSN_HR()
        {
            return @"EXEC SP_GET_MSN_HR;";
            //return @"SELECT [Id],[Data],[Year],[EmployeeID],[Name],[Gender],[DOB],[Age],[BU],[Entity],[Department],[Position]
            //                            ,[Rank],[RankGroup],[Function],[FunctionGroup],[MakeNonMake],[StartDate],[Location],[Contract],[EducationDegree]
            //                            ,[AgeGroupe],[LengthOfServiceGroup],[PMP],[9box] AS F9box,[DirectIndirect],[ProcessingMonth],[ProcessingYear]
            //                            FROM " + TableName + " (NOLOCK) A"
            //                          + " WHERE NOT EXISTS (SELECT 1 FROM MSN_Temp (NOLOCK) B WHERE A.Id = B.Id AND B.[TableName] = 'MSN_HR') ORDER BY A.Id";
        }
        private bool InsertMSNTemp(IDbConnection conn, List<MSN_Temp> MSN_Temps)
        {
            try
            {
                string query = @"INSERT INTO MSN_Temp ([TableName], Id, CrtDate) VALUES (@TableName, @Id, getdate())";
                if(MSN_Temps.Count > 0)
                {
                    List<MSN_Temp> dataInsert = new List<MSN_Temp>();
                    foreach(var temp in MSN_Temps)
                    {
                        dataInsert.Add(new MSN_Temp()
                        {
                            TableName = temp.TableName,
                            Id = temp.Id
                        });

                        if (dataInsert.Count == 1000)
                        {
                            conn.Execute(query, dataInsert);
                            FileHelper.WriteLogs("i = " + dataInsert.Count().ToString());
                            dataInsert.Clear();
                        }
                    }

                    if(dataInsert.Count > 0)
                    {
                        conn.Execute(query, dataInsert);
                        FileHelper.WriteLogs("InsertMSNTemp Inserted: " + dataInsert.Count().ToString());
                        dataInsert.Clear();
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    public class MSN_Temp
    {
        public string TableName { get; set; }
        public long Id { get; set; }
    }
}
