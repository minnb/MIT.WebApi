using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Central.AppService.Partner;
using PhucLong.Interface.Central.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Central.AppService
{
    public class InterfaceService
    {
        private readonly IConfiguration _config;
        private readonly List<InterfaceEntry> _interfaceEntry;
        private readonly CentralDbContext _dbContext;
        private readonly LoggingService _loggingService;
        public InterfaceService
            (
                IConfiguration config,
                List<InterfaceEntry> interfaceEntry,
                CentralDbContext dbContext
            )
        {
            _config = config;
            _interfaceEntry = interfaceEntry;
            _dbContext = dbContext;
            _loggingService = new LoggingService(_config);
        }
        public async Task RunInboundAsync(string set, string pathMasterData, string url_logging)
        {
            try
            {
                var lstInterface = _interfaceEntry.Where(x => x.JobType == "INB-" + set).OrderBy(x => x.SortOrder).ToList();
                if(lstInterface.Count > 0)
                {
                    foreach(var item in lstInterface)
                    {
                        long milliseconds = 0;
                        var st1 = new Stopwatch();
                        st1.Start();

                        await RunJob_InboundAsync(item, pathMasterData);
                        
                        milliseconds = st1.ElapsedMilliseconds;
                        st1.Stop();

                        _loggingService.LoggingElastic(url_logging, new LoggingElastic()
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            JobName = item.JobName,
                            IpAddress = NetworkHelper.GetLocalIPAddress(),
                            RunTime = milliseconds,
                            DeveloperMessage = "",

                        });
                    }
                }
                else
                {
                    FileHelper.WriteLogs("... nothing run");
                }

            }
            catch(Exception ex)
            {
                ExceptionHelper.WriteExptionError("RunInbound Exception", ex);
            }
        }
        [Obsolete]
        public void RunMasterData()
        {
            try
            {
                var lstInterface = _interfaceEntry.Where(x => x.JobType == "INB-MASTER").OrderBy(x => x.SortOrder).ToList();
                if (lstInterface.Count > 0)
                {
                    foreach (var item in lstInterface)
                    {
                        RunJob_InboundMasterData(item);
                    }
                }
                else
                {
                    FileHelper.WriteLogs("... nothing run");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("RunInbound Exception", ex);
            }
        }
        public async void RunOutbound(string set, string url_logging)
        {
            try
            {
                var lstInterface = _interfaceEntry.Where(x => x.JobType == "EXP-"+ set).OrderBy(x=>x.SortOrder).ToList();
                if (lstInterface.Count > 0)
                {
                    foreach (var item in lstInterface)
                    {
                        long milliseconds = 0;
                        var st1 = new Stopwatch();
                        st1.Start();

                        await RunJob_OutboundAsync(item);

                        milliseconds = st1.ElapsedMilliseconds;
                        st1.Stop();

                        _loggingService.LoggingElastic(url_logging, new LoggingElastic()
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            JobName = item.JobName,
                            IpAddress = NetworkHelper.GetLocalIPAddress(),
                            RunTime = milliseconds,
                            DeveloperMessage = "",

                        });
                    }
                }
                else
                {
                    FileHelper.WriteLogs("... nothing run");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("RunOutbound Exception", ex);
            }
        }
        public void UploadToSFTP(string set, string url_logging)
        {
            try
            {
                string jobType = "UPLOAD-" + set.ToUpper();
                var lstInterface = _interfaceEntry.Where(x => x.JobType == jobType).OrderBy(x => x.SortOrder).ToList();
                FileHelper.WriteLogs("===> START " + jobType);
                if (lstInterface.Count > 0)
                {
                    string[] scheduler = null;
                    foreach (var item in lstInterface)
                    {
                        long milliseconds = 0;
                        var st1 = new Stopwatch();
                        st1.Start();

                        if (!string.IsNullOrEmpty(item.Scheduler.ToString()))
                        {
                            scheduler = StringHelper.SliptString(item.Scheduler.ToString(), ";");
                        }
                        if(scheduler != null)
                        {
                            FileHelper.WriteLogs("===> Scheduler: " + JsonConvert.SerializeObject(scheduler));
                            if (scheduler.Contains(DateTime.Now.ToString("HH")))
                            {
                                SftpUpload(item, item.LocalPath.ToString(), item.LocalPathArchived.ToString(), item.PageSize);
                            }
                        }
                        else
                        {
                            SftpUpload(item, item.LocalPath.ToString(), item.LocalPathArchived.ToString(), item.PageSize);
                        }

                        milliseconds = st1.ElapsedMilliseconds;
                        st1.Stop();

                        _loggingService.LoggingElastic(url_logging, new LoggingElastic()
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            JobName = item.JobName,
                            IpAddress = NetworkHelper.GetLocalIPAddress(),
                            RunTime = milliseconds,
                            DeveloperMessage = "",

                        });
                    }
                }
                else
                {
                    FileHelper.WriteLogs("... nothing run");
                }
                FileHelper.WriteLogs("===> FINISHED " + jobType);
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("UploadToSFTP Exception", ex);
            }
        }
        private async Task RunJob_OutboundAsync(InterfaceEntry interfaceEntry)
        {
            string[] scheduler = null;
            string pathLocal = interfaceEntry.LocalPath.ToString();
            string pathAchive = interfaceEntry.LocalPathArchived.ToString();
            string pathError = interfaceEntry.SftpPath.ToString();
            int maxFile = interfaceEntry.MaxFile;
            bool isMove = interfaceEntry.IsMoveFile;
            string prefix = interfaceEntry.Prefix.ToString();
            string storeProcedure = interfaceEntry.StoreProName.ToString();

            if (!string.IsNullOrEmpty(interfaceEntry.Scheduler.ToString()))
            {
                scheduler = StringHelper.SliptString(interfaceEntry.Scheduler.ToString(), ";");
            }
            ExpSalesService expSalesService = new ExpSalesService(_config);
            FileHelper.WriteLogs("***** START: " + interfaceEntry.JobName + " *****");
            switch (interfaceEntry.JobName)
            {
                case "EXP-SALES":
                    expSalesService.Exp_Sales_V2(storeProcedure, pathLocal, prefix, interfaceEntry.IsMultiThread, interfaceEntry.PageSize, interfaceEntry.JobName);
                    //Upload sftp
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost.ToString()))
                    {
                        SftpUpload(interfaceEntry, pathLocal, pathAchive, interfaceEntry.PageSize);
                    }
                    break;

                case "EXP-RECONCILE":
                    if (scheduler!= null && scheduler.Contains(DateTime.Now.ToString("HH")))
                    {
                        expSalesService.Exp_Reconciliation(DateTime.Now.AddDays(-1).ToString("yyyyMMdd"), storeProcedure, pathLocal, prefix, interfaceEntry.IsMultiThread, interfaceEntry.PageSize);

                        //Upload sftp
                        if (!string.IsNullOrEmpty(interfaceEntry.SftpHost.ToString()))
                        {
                            SftpUpload(interfaceEntry, pathLocal, pathAchive, interfaceEntry.PageSize);
                        }
                    }

                    break;

                case "EXP-SALES-EXCEL":
                    if (scheduler != null && scheduler.Contains(DateTime.Now.ToString("HH")))
                    {
                        ExportExcelService exportExcelService = new ExportExcelService(_config);
                        var interfaceSetup = _dbContext.InterfaceSetup.Where(x => x.JobName == interfaceEntry.JobName && x.Blocked == false).ToList();
                        if(interfaceSetup.Count > 0)
                        {
                            foreach(var item in interfaceSetup)
                            {
                                exportExcelService.ExportSalesPhucLongToWCM(item.StoreProcedure, item.LocalPath, item.Prefix);
                                //Upload sftp
                                if (!string.IsNullOrEmpty(item.SftpHost.ToString()))
                                {
                                    SftpUploadSetup(item, item.LocalPath, item.LocalPathArchived);
                                }
                            }
                        }
                    }
                    break;

                case "EXP-FRANCHISE":
                    if (scheduler != null && scheduler.Contains(DateTime.Now.ToString("HH")))
                    {
                        expSalesService.Exp_FRANCHISE(DateTime.Now.AddDays(-1).ToString("yyyyMMdd"), storeProcedure, pathLocal, prefix);
                    }
                    //Upload sftp
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost.ToString()))
                    {
                        SftpUpload(interfaceEntry, pathLocal, pathAchive, interfaceEntry.PageSize);
                    }

                    break;

                case "EXP-SALES-GCP":
                    SelesGCPService transOCCService = new SelesGCPService(_config);
                    //OCC sales
                    transOCCService.GetSalesGCP_OCC(storeProcedure, pathLocal, maxFile);
                    //Upload
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost.ToString()))
                    {
                        SftpUpload(interfaceEntry, pathLocal, pathAchive, interfaceEntry.PageSize);
                    }
                    break;

                case "EXP-SALES-TO-GCP":
                    SelesGCPService transOCCService1 = new SelesGCPService(_config);
                    transOCCService1.GetSales_Exp_GCP(storeProcedure, pathLocal, maxFile, prefix);
                    //Upload
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost.ToString()))
                    {
                        SftpUpload(interfaceEntry, pathLocal, pathAchive, interfaceEntry.PageSize);
                    }

                    break;

                case "EXP-SAP-RESV-CR8":
                    if (scheduler != null && scheduler.Contains(DateTime.Now.ToString("HH")))
                    {
                        VoucherService sap1 = new VoucherService(_config);
                        sap1.PushSalesTrainningToSAP(interfaceEntry.StoreProName);
                    }
                    break;

                case "EXP-SALES-ESTELLA":
                    if (scheduler != null && scheduler.Contains(DateTime.Now.ToString("HH")))
                    {
                        ESTELLAService eSTELLAService = new ESTELLAService(_config);
                        if(eSTELLAService.SalesFileGeneration(interfaceEntry.LocalPath, interfaceEntry.StoreProName))
                        {
                            FileHelper.WriteLogs("===> Success");
                        }
                        //Upload sftp
                        if (!string.IsNullOrEmpty(interfaceEntry.SftpHost.ToString()))
                        {
                            SftpUpload(interfaceEntry, pathLocal, pathAchive, interfaceEntry.PageSize);
                        }
                    }
                    break;

                case "INB-JOBS":
                    JobsService jobs = new JobsService(_config);
                    if (scheduler != null && scheduler.Count() > 0)
                    {
                        if (scheduler.Contains(DateTime.Now.ToString("HH")))
                        {
                            await jobs.RunJobsAsync();
                        }
                    }
                    else
                    {
                        await jobs.RunJobsAsync();
                    }
                    break;
            }

            if (DateTime.Now.ToString("HH") == "06" && isMove == true)
            {
                FileHelper.WriteLogs("===> DELETE file in " + pathAchive + " @result: " + FileHelper.DeleteFileByDateHistory(pathAchive, maxFile * 100).ToString() + " file"); ;
            }
        }
        private async Task RunJob_InboundAsync(InterfaceEntry interfaceEntry, string pathMasterData)
        {
            string[] scheduler = null;
            string pathLocal = interfaceEntry.LocalPath.ToString();
            string pathAchive = interfaceEntry.LocalPathArchived.ToString();
            string pathError = interfaceEntry.SftpPath.ToString();
            int maxFile = interfaceEntry.MaxFile;
            bool isMove = interfaceEntry.IsMoveFile;
            string storeProcedure = interfaceEntry.StoreProName.ToString();

            if (!string.IsNullOrEmpty(interfaceEntry.Scheduler.ToString()))
            {
                scheduler = StringHelper.SliptString(interfaceEntry.Scheduler.ToString(), ";");
            }

            FileHelper.WriteLogs("***** START: " + interfaceEntry.JobName + " *****");
            switch (interfaceEntry.JobName)
            {
                case "INB-SALES-V1":
                    TransOldAppService transOldAppService = new TransOldAppService(_config, _dbContext);
                    FileHelper.WriteLogs("===> Processed: " + transOldAppService.SaveTransaction_V1(pathMasterData, pathLocal, pathAchive, pathError, maxFile, isMove).ToString() + " file");
                    break;

                case "INB-SALES-OCC":
                    string connectString = interfaceEntry.Prefix.ToString();
                    if (!string.IsNullOrEmpty(connectString))
                    {
                        TransOCCService transOCCService = new TransOCCService(_config, connectString, _dbContext);
                        transOCCService.GetTransFromBLUEPOS(storeProcedure, pathLocal);
                        Thread.Sleep(500);
                        transOCCService.SaveOCCTransaction_V1(pathLocal, pathAchive, pathError, maxFile, isMove);
                    }                   
                    break;

                case "INB-VOUCHER":
                    VoucherService voucherService = new VoucherService(_config);
                    voucherService.SaveVoucherInfo(pathLocal, pathAchive, pathError, maxFile, isMove);
                    
                    Thread.Sleep(200);
                    voucherService.UpdateStatusVoucherToCX(storeProcedure, isMove);
                    break;

                case "INB-VOUCHER-SAP":
                    VoucherService sap = new VoucherService(_config);
                    sap.UpdateStatusVoucherToSAP(storeProcedure);
                    break;
                case "INB-LOYALTY":
                    LoyaltyService loyaltyService = new LoyaltyService(_config);
                    if (!string.IsNullOrEmpty(interfaceEntry.StoreProName))
                    {
                        loyaltyService.GetDataRawLoyaltyAsync(interfaceEntry.StoreProName);
                    }
                    else
                    {
                        FileHelper.WriteLogs("Not found StoreProcedure");
                    }
                    break;

                case "INB-JOBS":
                    JobsService jobs = new JobsService(_config);
                    if(scheduler != null && scheduler.Count() > 0)
                    {
                        if (scheduler.Contains(DateTime.Now.ToString("HH")))
                        {
                            await jobs.RunJobsAsync();
                        }
                    }
                    else
                    {
                        await jobs.RunJobsAsync();
                    }
                    break;

                case "INB-SAP-RESV-CR8":
                    if (scheduler.Contains(DateTime.Now.ToString("HH")))
                    {
                        VoucherService sap1 = new VoucherService(_config);
                        sap1.PushSalesTrainningToSAP(interfaceEntry.StoreProName);
                    }
                    break;

                case "INB-GCP-SURVEY-RESULT":
                    SelesGCPService sry = new SelesGCPService(_config);
                    sry.GetDataSurveyResult(interfaceEntry.StoreProName);
                    break;
            }

            if (DateTime.Now.ToString("HH") == "06" && isMove == true)
            {
                FileHelper.WriteLogs("DELETE file in " + pathAchive + " @result: " + FileHelper.DeleteFileByDateHistory(pathAchive, maxFile * 200).ToString() + " file"); 
            }
        }
        private void SftpUpload(InterfaceEntry interfaceEntry, string pathLocal, string pathAchive, int pageSize)
        {
            string sftpOS = interfaceEntry.SftpOS.ToString();
            string sftpHost = interfaceEntry.SftpHost.ToString();
            int sftpPort = interfaceEntry.SftpPort;
            string sftpUserName = interfaceEntry.SftpUser.ToString();
            string sftpPassword = interfaceEntry.SftpPass.ToString();
            string sftpPath = interfaceEntry.SftpPath.ToString();
            string fileExtention = interfaceEntry.FileExtension.ToString();
            short sftpPermision = (Int16)interfaceEntry.SfptPermissions;
            bool isMoveFile = interfaceEntry.IsMoveFile;

            if (!string.IsNullOrEmpty(sftpHost))
            {
                try
                {
                    SftpHelper sftpHelper = new SftpHelper(sftpHost, sftpPort, sftpUserName, sftpPassword);
                    if (sftpOS.ToUpper() == "WINDOW")
                    {
                        sftpHelper.UploadSftpWindow(pathLocal, sftpPath, pathAchive, fileExtention, sftpPermision, isMoveFile, pageSize);
                    }
                    else
                    {
                        if (sftpOS.ToUpper() == "LINUX")
                        {
                            sftpHelper.UploadSftpLinux(pathLocal, sftpPath, pathAchive, fileExtention, sftpPermision, isMoveFile);
                        }
                        else
                        {
                            sftpHelper.UploadSftpLinux2(pathLocal, sftpPath, pathAchive, fileExtention, sftpPermision, isMoveFile);
                        }
                    }
                }
                catch(Exception ex)
                {
                    FileHelper.WriteLogs("===> SftpUpload Exception " + ex.Message.ToString());
                }
            }
        }
        private void SftpDownload(InterfaceEntry interfaceEntry)
        {
            string sftpOS = interfaceEntry.SftpOS.ToString();
            string sftpHost = interfaceEntry.SftpHost.ToString();
            int sftpPort = interfaceEntry.SftpPort;
            string sftpUserName = interfaceEntry.SftpUser.ToString();
            string sftpPassword = interfaceEntry.SftpPass.ToString();
            string sftpPath = interfaceEntry.SftpPath.ToString();
            string pathLocal = interfaceEntry.LocalPath.ToString();
            if (!string.IsNullOrEmpty(sftpHost))
            {
                FileHelper.WriteLogs("***** Host sftp: " + sftpHost);
                FileHelper.WriteLogs("***** Download file from sftp: " + sftpPath);
                SftpHelper sftpHelper = new SftpHelper(sftpHost, sftpPort, sftpUserName, sftpPassword);
                if (sftpOS.ToUpper() == "AUTHEN")
                {
                    sftpHelper.DownloadAuthen(sftpPath, pathLocal);
                }
                else
                {
                    sftpHelper.DownloadNoAuthen(sftpPath, pathLocal, true);
                }
            }
        }
        [Obsolete]
        private void RunJob_InboundMasterData(InterfaceEntry interfaceEntry)
        {
            string[] scheduler = null;
            string pathLocal = interfaceEntry.LocalPath.ToString();
            string pathAchive = interfaceEntry.LocalPathArchived.ToString();
            string pathError = interfaceEntry.SftpPath.ToString();
            int maxFile = interfaceEntry.MaxFile;
            bool isMove = interfaceEntry.IsMoveFile;
            string prefix = interfaceEntry.Prefix.ToString();
            string storeProcedure = interfaceEntry.StoreProName.ToString();

            if (!string.IsNullOrEmpty(interfaceEntry.Scheduler.ToString()))
            {
                scheduler = StringHelper.SliptString(interfaceEntry.Scheduler.ToString(), ";");
            }

            FileHelper.WriteLogs("***** START: " + interfaceEntry.JobName + " *****");
            switch (interfaceEntry.JobName)
            {
                case "INB-ITEM":                   
                    ItemMasterService itemMasterService = new ItemMasterService(_config,_dbContext);
                    SftpDownload(interfaceEntry);
                    FileHelper.WriteLogs("===> start processing file xml");
                    itemMasterService.SaveItemMaster(pathLocal, pathAchive, maxFile, isMove, storeProcedure);
                    Thread.Sleep(500);
                    FileHelper.WriteLogs("===> start processing temp_item");
                    if (!string.IsNullOrEmpty(storeProcedure))
                    {
                        Hashtable hashtable = new Hashtable();
                        hashtable.Add("@UpdateFlg", "N");
                        FileHelper.WriteLogs("===> EXEC " + storeProcedure);
                        _dbContext.ExecuteProcedure(storeProcedure, hashtable);
                    }
                    FileHelper.WriteLogs("===> finished");
                    break;
            }

        }
        private void SftpUploadSetup(InterfaceSetup interfaceEntry, string pathLocal, string pathAchive)
        {
            string sftpOS = interfaceEntry.SftpOS.ToString();
            string sftpHost = interfaceEntry.SftpHost.ToString();
            int sftpPort = interfaceEntry.SftpPort;
            string sftpUserName = interfaceEntry.SftpUser.ToString();
            string sftpPassword = interfaceEntry.SftpPass.ToString();
            string sftpPath = interfaceEntry.SftpPath.ToString();
            string fileExtention = interfaceEntry.FileExtension.ToString();
            short sftpPermision = (Int16)interfaceEntry.SfptPermissions;
            bool isMoveFile = interfaceEntry.IsMoveFile;

            if (!string.IsNullOrEmpty(sftpHost))
            {
                FileHelper.WriteLogs("***** Upload file to sftp *****");
                FileHelper.WriteLogs("===> Host: " + sftpHost + ":" + sftpPort);
                FileHelper.WriteLogs("===> File: " + fileExtention  + " from: " + pathLocal);
                FileHelper.WriteLogs("===> Sftp: " + sftpPath);
                SftpHelper sftpHelper = new SftpHelper(sftpHost, sftpPort, sftpUserName, sftpPassword);
                if (sftpOS.ToUpper() == "WINDOW")
                {
                    sftpHelper.UploadSftpWindow(pathLocal, sftpPath, pathAchive, fileExtention, sftpPermision, isMoveFile);
                }
                else
                {
                    if (sftpOS.ToUpper() == "LINUX")
                    {
                        sftpHelper.UploadSftpLinux(pathLocal, sftpPath, pathAchive, fileExtention, sftpPermision, isMoveFile);
                    }
                    else
                    {
                        sftpHelper.UploadSftpLinux2(pathLocal, sftpPath, pathAchive, fileExtention, sftpPermision, isMoveFile);
                    }

                }
            }
        }

    }
}
