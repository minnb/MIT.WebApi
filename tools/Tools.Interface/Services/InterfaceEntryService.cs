using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tools.Common.Helpers;
using Tools.Interface.Database;
using Tools.Interface.Services.DrWin;
using Tools.Interface.Services.GCP;
using Tools.Interface.Services.Shopee;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;

namespace Tools.Interface.Services
{
    public class InterfaceEntryService
    {
        private IConfiguration _config;
        private List<InterfaceEntry> _interfaceEntry;
        private InterfaceDbContext _dbContext;
        private string _stringConnectDbInterface = string.Empty;
        public InterfaceEntryService
            (
                IConfiguration config,
                List<InterfaceEntry> interfaceEntry,
                InterfaceDbContext dbContext
            )
        {
            _interfaceEntry = interfaceEntry;
            _dbContext = dbContext;
            _config = config;
            _stringConnectDbInterface = _config["ConnectionStrings:Default"].ToString();
        }
        public async Task RunInterfaceAsync(string appCode, string jobName)
        {
            try
            {
                Console.WriteLine("Run: {0}", appCode);
                var lstInterface = _interfaceEntry.Where(x => x.AppCode == appCode && x.Blocked == false).OrderBy(x => x.SortOrder).ToList();
                if (lstInterface.Count > 0)
                {
                    switch (appCode)
                    {
                        case "OMC":
                            foreach (var items in lstInterface)
                            {
                                await Run_Job_VINID(items);
                            }
                            break;
                        case "GCP":
                            var lstRun = lstInterface.Where(x => x.JobType == jobName).OrderBy(x => x.SortOrder).ToList();
                            foreach (var items in lstRun)
                            {
                                Run_Job_GCP(items);
                            }
                            break;
                        case "DATALAKE":
                            var lstRun1 = lstInterface.Where(x => x.JobType == jobName).OrderBy(x => x.SortOrder).ToList();
                            foreach (var items in lstRun1)
                            {
                                Run_Job_DATALAKE(items);
                            }
                            break;
                        case "KIOS":
                            var lstRunKios = lstInterface.Where(x => x.JobType == jobName).OrderBy(x => x.SortOrder).ToList();
                            foreach (var items in lstRunKios)
                            {
                                Run_Job_KIOS(items);
                            }
                            break;
                        case "NOW":
                            var lstRunNowFood = lstInterface.Where(x => x.JobType == jobName).OrderBy(x => x.SortOrder).ToList();
                            foreach (var items in lstRunNowFood)
                            {
                                Run_Job_NOWFOOD(items);
                            }
                            break;
                        case "DRW":
                            var lstRunJobDrW = lstInterface.Where(x => x.JobType == jobName).OrderBy(x => x.SortOrder).ToList();
                            foreach (var items in lstRunJobDrW)
                            {
                                Run_Job_DRWIN(items);
                            }
                            break;
                        case "WMC":
                            var lstRunJobWMC = lstInterface.Where(x => x.JobType == jobName).OrderBy(x => x.SortOrder).ToList();
                            foreach (var items in lstRunJobWMC)
                            {
                                Run_Job_WMC(items);
                            }
                            break;
                    }                   
                }
                else
                {
                    FileHelper.WriteLogs("===> ... nothing run");
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("===> RunInterfaceAsync Exception", ex);
            }
        }
        private async Task Run_Job_VINID(InterfaceEntry interfaceEntry)
        {
            VINIDReconService vINIDReconService = new VINIDReconService();
            FileHelper.WriteLogs("===> RUN: " + interfaceEntry.JobName);
            switch (interfaceEntry.JobName)
            {
                case "RECON-TOPUP":
                    await vINIDReconService.Exp_ReconciliationTopUp(_stringConnectDbInterface, interfaceEntry.LocalPath, interfaceEntry.Prefix);
                    break;
                case "":
                    break;
               
            }

            if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
            {
                InterfaceHelper.UploadFileToSftpSetup(interfaceEntry);
            }
        }
        private void Run_Job_GCP(InterfaceEntry interfaceEntry)
        {
            GCPService gCPService = new GCPService();
            FileHelper.WriteLogs("===> RUN JobType: " + interfaceEntry.JobType + " - JobName: " + interfaceEntry.JobName);
            switch (interfaceEntry.JobName)
            {
                case "PRICE-RATE-DAILY":
                    gCPService.ProcessPriceRateDaily_GCP(interfaceEntry.Prefix);
                    break;
                case "SALE-OUT":
                    //(string connectionString, string procedure, string pathLocal, string prefix, string batchJob, int max_rows)
                    string batchJob = interfaceEntry.JobType + "_" +DateTime.Now.ToString("yyyyMMddHHmmssf");
                    gCPService.Exp_Trans_WCM_To_GCP(interfaceEntry.Prefix, interfaceEntry.StoreProName, interfaceEntry.LocalPath, batchJob, interfaceEntry.MaxFile);
                    break;
                case "TRANS_VOID":
                    gCPService.ExpTransVoid(interfaceEntry.AppCode, interfaceEntry.Prefix, interfaceEntry.StoreProName, interfaceEntry.JobType, interfaceEntry.LocalPath);
                    break;
                case "RUN-PRO":
                    gCPService.Run_StoreProcedure(interfaceEntry.Prefix, interfaceEntry.StoreProName, interfaceEntry.JobType);
                    break;
                case "CALL-API":
                    gCPService.Push_TransWCM_To_GCP(interfaceEntry.LocalPath, interfaceEntry.LocalPathArchived, interfaceEntry.SftpPath, interfaceEntry.IsMoveFile);
                    break;
                case "CALL-API-PLH":
                    gCPService.Push_Trans_PLH_GCP(interfaceEntry.LocalPath, interfaceEntry.LocalPathArchived, interfaceEntry.IsMoveFile, interfaceEntry.MaxFile);
                    break;
                case "CALL-API-SUMMARY":
                    gCPService.Push_Summary_SalesOut_WCM(interfaceEntry.Prefix, interfaceEntry.StoreProName, interfaceEntry.LocalPath);
                    break;
                case "SUMMARY-BILL":
                    gCPService.ExpCountTotalBill(interfaceEntry.JobType.Substring(0,3).ToUpper(), interfaceEntry.Prefix, interfaceEntry.StoreProName, interfaceEntry.JobType, interfaceEntry.LocalPath);
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.UploadFileToSftpSetup(interfaceEntry);
                    }
                    break;
                case "RECEIPT_NO":
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.DownloadFileToSftpSetup(interfaceEntry);
                    }
                    gCPService.Import_ReceiptNo_GCP_To_SET1(interfaceEntry.Prefix, interfaceEntry.LocalPath, interfaceEntry.LocalPathArchived, interfaceEntry.IsMoveFile);
                    break;
                case "EXP-HCM":
                    gCPService.ExpHR_To_GCP(interfaceEntry.AppCode, interfaceEntry.Prefix, interfaceEntry.JobType, interfaceEntry.LocalPath, interfaceEntry.LocalPathArchived, interfaceEntry.IsMoveFile);
                    break;

                //SAP HCM
                case "DASHBOARD":
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.DownloadFileToSftpSetup(interfaceEntry);
                    }
                    SAPHCM_DashboardService sAPHCM_DashboardService = new SAPHCM_DashboardService();
                    sAPHCM_DashboardService.Save_SAPHCM_Dashboard(interfaceEntry.JobName, interfaceEntry.Prefix, interfaceEntry.LocalPath, interfaceEntry.MaxFile);
                    break;
            }

            //if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
            //{
            //    InterfaceHelper.UploadFileToSftpSetup(interfaceEntry);
            //}
        }
        private void Run_Job_KIOS(InterfaceEntry interfaceEntry)
        {
            Console.WriteLine("Run: {0}", interfaceEntry.JobName);
            FileHelper.WriteLogs("===> RUN: " + interfaceEntry.JobName);
            KiosService kiosService = new KiosService();
            switch (interfaceEntry.JobName)
            {
                case "EXP-TRANS-RAW":
                    kiosService.ExpTransFromKios(interfaceEntry.AppCode, interfaceEntry.Prefix, interfaceEntry.LocalPath);
                    break;
                case "IMPORT-TRANS-WCM":
                    kiosService.ImportTransWinmartToDB("WCM", interfaceEntry.Prefix, interfaceEntry.LocalPath, interfaceEntry.LocalPathArchived, interfaceEntry.SftpPath, interfaceEntry.IsMoveFile, interfaceEntry.MaxFile);
                    break;
                case "IMPORT-TRANS-PLH":
                    kiosService.ImportTransPhucLongToDB("PLH", interfaceEntry.Prefix, interfaceEntry.LocalPath, interfaceEntry.LocalPathArchived, interfaceEntry.SftpPath, interfaceEntry.IsMoveFile, interfaceEntry.MaxFile);
                    break;
                case "IMPORT-TRANS-V2":
                    kiosService.ProcessingSalesKIOS(interfaceEntry.Prefix, interfaceEntry.LocalPath, interfaceEntry.LocalPathArchived, interfaceEntry.SftpPath, interfaceEntry.IsMoveFile, interfaceEntry.MaxFile);
                    break;
            }
            if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
            {
                InterfaceHelper.UploadFileToSftpSetup(interfaceEntry);
            }
        }
        private void Run_Job_NOWFOOD(InterfaceEntry interfaceEntry)
        {
            string[] scheduler = null;
            if (!string.IsNullOrEmpty(interfaceEntry.Scheduler.ToString()))
            {
                scheduler = StringHelper.SliptString(interfaceEntry.Scheduler.ToString(), ";");
            }

            Console.WriteLine("Run: {0}", interfaceEntry.JobName);
            FileHelper.WriteLogs("===> RUN: " + interfaceEntry.JobName);
            switch (interfaceEntry.JobName)
            {
                case "SYNC-DISH":
                    DishService dishService = new DishService(interfaceEntry);
                    //Create dish to NOWFOOD
                    dishService.CreateShopeeDish(interfaceEntry.JobName.ToString());

                    //mapping dish-topping set group quantity
                    //dishService.DishToppingMappingCreate(interfaceEntry.JobName.ToString());
                    //dishService.DishToppingSetGroupQuantity(interfaceEntry.JobName.ToString());
                    break;
                case "SYNC-TOPPING":
                    ToppingService toppingService = new ToppingService(interfaceEntry);                   
                    toppingService.CreateShopeeTopping(interfaceEntry.JobName.ToString());

                    break;
                case "SYNC-MAPPING":
                    //sync mapping STORE
                    NowService nowService = new NowService(interfaceEntry);
                    nowService.get_restaurant_info(interfaceEntry.JobName.ToString());

                    Thread.Sleep(100);
                    //mapping
                    MappingService mappingService = new MappingService(interfaceEntry);
                    mappingService.RunMappingPartner(interfaceEntry.JobName.ToString());
                    break;
                case "SYNC-DATA":
                    if (scheduler != null && scheduler.Contains(DateTime.Now.ToString("HH")))
                    {
                        DishService dishService1 = new DishService(interfaceEntry);
                        //Sync data from NOWFOOD
                        dishService1.GetShopeeDishGroup(interfaceEntry.JobName.ToString());
                        dishService1.GetShopeeDish(interfaceEntry.JobName.ToString());
                        
                        Thread.Sleep(500);
                        ToppingService toppingService1 = new ToppingService(interfaceEntry);
                        //Get topping
                        toppingService1.GetShopeeTopping(interfaceEntry.JobName.ToString());
                        toppingService1.GetShopeeToppingGroup(interfaceEntry.JobName.ToString());
                    }
                    break;
            }

            Console.WriteLine("JobName: {0}", interfaceEntry.JobName);
            if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
            {
                InterfaceHelper.UploadFileToSftpSetup(interfaceEntry);
            }

        }
        private void Run_Job_DRWIN(InterfaceEntry interfaceEntry)
        {
            string[] scheduler = null;
            if (!string.IsNullOrEmpty(interfaceEntry.Scheduler.ToString()))
            {
                scheduler = StringHelper.SliptString(interfaceEntry.Scheduler.ToString(), ";");
            }

            Console.WriteLine("Run: {0}", interfaceEntry.JobName);
            FileHelper.WriteLogs("===> RUN: " + interfaceEntry.JobName);
            DrWinService service = new DrWinService(interfaceEntry);
            switch (interfaceEntry.JobName)
            {
                case "POST-SALES":
                    service.PostHoaDonThuoc_To_CucDuoc(interfaceEntry.JobName.ToString(), interfaceEntry.StoreProName);
                    break;
                case "GET-SALES":
                    service.GetSaleFromCentral(interfaceEntry.JobName.ToString(), interfaceEntry.StoreProName);
                    break;
                case "INB-STOCK":
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.DownloadFileToSftpSetup(interfaceEntry);
                    }
                    service.ProcessInboundStock(interfaceEntry.JobName.ToString(), interfaceEntry.LocalPath, interfaceEntry.LocalPathArchived, interfaceEntry.MaxFile, interfaceEntry.StoreProName);
                    break;
            }
        }
        private void Run_Job_WMC(InterfaceEntry interfaceEntry)
        {
            if (!string.IsNullOrEmpty(interfaceEntry.Scheduler.ToString()))
            {
                string[] scheduler = StringHelper.SliptString(interfaceEntry.Scheduler.ToString(), ";");
            }

            Console.WriteLine("Run: {0}", interfaceEntry.JobName);
            FileHelper.WriteLogs("===> RUN: " + interfaceEntry.JobName);
            WMCService service = new WMCService(interfaceEntry);
            switch (interfaceEntry.JobName)
            {
                case "IMPORT-TRANS":
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.DownloadFileToSftpSetup(interfaceEntry);
                    }
                    service.ImportWinMoneyTrans(interfaceEntry);
                    break;

            }
        }

        private void Run_Job_DATALAKE(InterfaceEntry interfaceEntry)
        {
            if (!string.IsNullOrEmpty(interfaceEntry.Scheduler.ToString()))
            {
                string[] scheduler = StringHelper.SliptString(interfaceEntry.Scheduler.ToString(), ";");
            }

            Console.WriteLine("Run: {0}", interfaceEntry.JobName);
            FileHelper.WriteLogs("===> RUN: " + interfaceEntry.JobName);
            GCP_MD_Service service = new GCP_MD_Service();
            switch (interfaceEntry.JobName)
            {
                case "GCP_PRODUCT_UNIT":
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.DownloadFileToSftpSetup(interfaceEntry);
                    }
                    service.GCP_PRODUCT_UNIT(interfaceEntry);
                    break;
                case "GCP_PRODUCT":
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.DownloadFileToSftpSetup(interfaceEntry);
                    }
                    service.GCP_PRODUCT(interfaceEntry);
                    break;
                case "GCP_MCH_INFO":
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.DownloadFileToSftpSetup(interfaceEntry);
                    }
                    service.GCP_MCH_INFO(interfaceEntry);
                    break;
                case "GCP_STORE":
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.DownloadFileToSftpSetup(interfaceEntry);
                    }
                    service.GCP_STORE(interfaceEntry);
                    break;
                case "GCP_SALE_BY_BU_REGION":
                    if (!string.IsNullOrEmpty(interfaceEntry.SftpHost))
                    {
                        InterfaceHelper.DownloadFileToSftpSetup(interfaceEntry);
                    }
                    service.GCP_SALE_BY_BU_REGION(interfaceEntry);
                    break;
            }
        }
    }
}
