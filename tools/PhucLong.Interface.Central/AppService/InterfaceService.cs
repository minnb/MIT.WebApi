using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Central.AppService
{
    public class InterfaceService
    {
        private IConfiguration _config;
        private List<InterfaceEntry> _interfaceEntry;
        private CentralDbContext _dbContext;
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
        }

        public void RunInbound(string set, string pathMasterData)
        {
            try
            {
                var lstInterface = _interfaceEntry.Where(x => x.JobType == "INB-" + set).OrderBy(x => x.SortOrder).ToList();
                if(lstInterface.Count > 0)
                {
                    foreach(var item in lstInterface)
                    {
                        RunJob_Inbound(item, pathMasterData);
                    }
                }
            }
            catch(Exception ex)
            {
                ExceptionHelper.WriteExptionError("RunInbound Exception", ex);
            }
        }

        public void RunOutbound(string set)
        {
            try
            {
                var lstInterface = _interfaceEntry.Where(x => x.JobType == "EXP-"+ set).OrderBy(x=>x.SortOrder).ToList();
                if (lstInterface.Count > 0)
                {
                    foreach (var item in lstInterface)
                    {
                        RunJob_Outbound(item);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("RunOutbound Exception", ex);
            }
        }
        private void RunJob_Outbound(InterfaceEntry interfaceEntry)
        {
            string pathLocal = interfaceEntry.LocalPath.ToString();
            string pathAchive = interfaceEntry.LocalPathArchived.ToString();
            string pathError = interfaceEntry.SftpPath.ToString();
            int maxFile = interfaceEntry.MaxFile;
            bool isMove = interfaceEntry.IsMoveFile;
            string prefix = interfaceEntry.Prefix.ToString();
            string storeProcedure = interfaceEntry.StoreProName.ToString();

            FileHelper.WriteLogs("***** START: " + interfaceEntry.JobName + " *****");
            ExpSalesService expSalesService = new ExpSalesService(_config);
            switch (interfaceEntry.JobName)
            {
                case "EXP-SALES":

                    expSalesService.Exp_Sales(storeProcedure, pathLocal, prefix);

                    //Upload sftp
                    FileHelper.WriteLogs("***** Upload file to sftp *****");
                    SftpUpload(interfaceEntry, pathLocal, pathAchive);
                    break;

                case "EXP-RECONCILE":
                    expSalesService.Exp_Reconciliation(pathLocal, prefix);

                    //Upload sftp
                    //FileHelper.WriteLogs("***** Upload file to sftp *****");
                    //SftpUpload(interfaceEntry, pathLocal, pathAchive);
                    break;
            }

        }
        private void RunJob_Inbound(InterfaceEntry interfaceEntry, string pathMasterData)
        {
            string pathLocal = interfaceEntry.LocalPath.ToString();
            string pathAchive = interfaceEntry.LocalPathArchived.ToString();
            string pathError = interfaceEntry.SftpPath.ToString();
            int maxFile = interfaceEntry.MaxFile;
            bool isMove = interfaceEntry.IsMoveFile;
            string storeProcedure = interfaceEntry.StoreProName.ToString();

            FileHelper.WriteLogs("***** START: " + interfaceEntry.JobName + " *****");
            switch (interfaceEntry.JobName)
            {
                case "INB-SALES-V1":
                    TransOldAppService transOldAppService = new TransOldAppService(_config, _dbContext);
                    transOldAppService.SaveTransaction_V1(pathMasterData, pathLocal, pathAchive, pathError, maxFile, isMove);
                    break;

                case "INB-VOUCHER":
                    VoucherService voucherService = new VoucherService(_config);
                    voucherService.SaveVoucherInfo(pathLocal, pathAchive, pathError, maxFile, isMove);
                    
                    Thread.Sleep(200);
                    voucherService.UpdateStatusVoucherToCX(storeProcedure);
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
            }
            FileHelper.WriteLogs("***** =============END============= *****");
        }

        private void SftpUpload(InterfaceEntry interfaceEntry, string pathLocal, string pathAchive)
        {
            string sftpOS = interfaceEntry.SftpOS.ToString();
            string sftpHost = interfaceEntry.SftpHost.ToString();
            int sftpPort = interfaceEntry.SftpPort;
            string sftpUserName = interfaceEntry.SftpUser.ToString();
            string sftpPassword = interfaceEntry.SftpPass.ToString();
            string sftpPath = interfaceEntry.SftpPath.ToString();
            string fileExtention = interfaceEntry.FileExtension.ToString();
            int sftpPermision = interfaceEntry.SfptPermissions;
            if (!string.IsNullOrEmpty(sftpHost))
            {
                SftpHelper sftpHelper = new SftpHelper(sftpHost, sftpPort, sftpUserName, sftpPassword);
                if(sftpOS.ToUpper() == "WINDOW")
                {
                    sftpHelper.UploadSftpWindow(pathLocal, sftpPath, pathAchive, fileExtention);
                }
                else
                {
                    sftpHelper.UploadSftpLinux(pathLocal, sftpPath, pathAchive, fileExtention);
                }
            }
        }
    }
}
