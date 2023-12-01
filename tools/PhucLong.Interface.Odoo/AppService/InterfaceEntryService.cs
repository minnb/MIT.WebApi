using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Odoo.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Odoo.AppService
{
    public class InterfaceEntryService
    {
        private IConfiguration _config;
        public InterfaceEntryService
            (
                IConfiguration config
            )
        {
            _config = config;
        }

        public void RunInterfaceEntry(string set, string back_date)
        {
            ConndbCentral _dbContext = new ConndbCentral(_config);
            using var conn = _dbContext.ConnCentralPhucLong;
            conn.Open();
            try
            {
                var lstInterface = _dbContext.GetInterfaceEntry(conn, "ODOO-" + set);
                if (lstInterface.Count > 0)
                {
                    foreach (var item in lstInterface)
                    {
                        if(item.JobName == "ODOO-API")
                        {
                            WebApiService webApiService = new WebApiService(_config);
                            webApiService.CallApi(conn, item);
                        }
                        else
                        {
                            RunJob(item, back_date, set);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("RunInterfaceEntry Exception", ex);
            }
        }
        private void RunJob(InterfaceEntry interfaceEntry, string back_date, string set)
        {
            string[] scheduler = null;
            string pathLocal = interfaceEntry.LocalPath.ToString();
            string pathAchive = interfaceEntry.LocalPathArchived.ToString();
            string pathError = interfaceEntry.SftpPath.ToString();
            int maxFile = interfaceEntry.MaxFile;
            bool isMove = interfaceEntry.IsMoveFile;
            string connectString = interfaceEntry.Prefix.ToString();

            if (!string.IsNullOrEmpty(interfaceEntry.Scheduler.ToString()))
            {
                scheduler = StringHelper.SliptString(interfaceEntry.Scheduler.ToString(), ";");
            }

            FileHelper.WriteLogs("***** START: " + interfaceEntry.JobName + " *****");           
            switch (interfaceEntry.JobName)
            {
                case "ODOO-SALES":
                    PosOrderService posOrderService = new PosOrderService(_config);
                    posOrderService.GetDataOdoo(connectString, back_date, maxFile, pathLocal, true, 0);

                    Thread.Sleep(200);
                    FileHelper.WriteLogs("***** GET POS_ORDER_CANCEL *****");
                    posOrderService.Get_pos_order_cancel(connectString, back_date, pathLocal);

                    if (DateTime.Now.ToString("HH") == "22" || DateTime.Now.ToString("HH") == "23")
                    {
                        FileHelper.WriteLogs("***** GET POS_VAT *****");
                        posOrderService.Get_pos_order_request_vat(connectString, back_date, pathLocal);
                    }

                    if (scheduler != null)
                    {
                        int order_id = int.Parse(scheduler[0].ToString());
                        FileHelper.WriteLogs("***** pos_order historic: " + order_id.ToString());
                        posOrderService.GetDataOdoo(connectString, back_date, maxFile, pathLocal, false, order_id);
                    }
                    break;

                case "ODOO-MD":
                    if (scheduler.Contains(DateTime.Now.ToString("HH")))
                    {
                        FileHelper.WriteLogs("Scheduler: " + JsonConvert.SerializeObject(scheduler));
                        MasterAppService masterAppService = new MasterAppService(_config);
                        masterAppService.GetMasterDataOdoo(connectString, pathLocal);
                    }
                    break;

                case "ODOO-VOUCHER":
                    VoucherService voucherService = new VoucherService(_config);
                    voucherService.GetVoucherInfoOdoo(connectString, back_date, maxFile, pathLocal);
                    break;

                case "ODOO-SUMMARY":
                    if (scheduler.Contains(DateTime.Now.ToString("HH")))
                    {
                        Console.WriteLine("SavePaymentReconcileOdoo");
                        SummaryService summaryService = new SummaryService(_config);
                        summaryService.SavePaymentReconcileOdoo(interfaceEntry.Prefix, interfaceEntry.LocalPath, set);
                        Thread.Sleep(500);
                        Console.WriteLine("SaveSalesReconcileOdoo");
                        summaryService.SaveSalesReconcileOdoo(interfaceEntry.Prefix, interfaceEntry.LocalPath, set);
                    }
                    break;
            }
            FileHelper.WriteLogs("***** FINISHED " + interfaceEntry.JobName + " *****");
        }
    }
}
