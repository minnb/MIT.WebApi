using Microsoft.Extensions.Configuration;
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
                        RunJob(item, back_date);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("RunInterfaceEntry Exception", ex);
            }
        }
        private void RunJob(InterfaceEntry interfaceEntry, string back_date)
        {
            string pathLocal = interfaceEntry.LocalPath.ToString();
            string pathAchive = interfaceEntry.LocalPathArchived.ToString();
            string pathError = interfaceEntry.SftpPath.ToString();
            int maxFile = interfaceEntry.MaxFile;
            bool isMove = interfaceEntry.IsMoveFile;
            string connectString = interfaceEntry.Prefix.ToString();

            FileHelper.WriteLogs("***** START: " + interfaceEntry.JobName + " *****");           
            switch (interfaceEntry.JobName)
            {
                case "ODOO-SALES":
                    PosOrderService posOrderService = new PosOrderService(_config);
                    posOrderService.GetDataOdoo(connectString, back_date, maxFile, pathLocal, true);

                    Thread.Sleep(200);
                    FileHelper.WriteLogs("***** GET POS_ORDER_CANCEL *****");
                    posOrderService.Get_pos_order_cancel(connectString, back_date, pathLocal);

                    Thread.Sleep(200);
                    FileHelper.WriteLogs("***** GET POS_VAT *****");
                    posOrderService.Get_pos_order_request_vat(connectString, back_date, pathLocal);

                    break;
                case "ODOO-MD":
                    MasterAppService masterAppService = new MasterAppService(_config);
                    masterAppService.GetMasterDataOdoo(connectString, pathLocal);

                    break;
                case "ODOO-VOUCHER":
                    VoucherService voucherService = new VoucherService(_config);
                    voucherService.GetVoucherInfoOdoo(connectString, back_date, maxFile, pathLocal);
                    break;
            }
            FileHelper.WriteLogs("***** FINISHED " + interfaceEntry.JobName + " *****");
        }
    }
}
