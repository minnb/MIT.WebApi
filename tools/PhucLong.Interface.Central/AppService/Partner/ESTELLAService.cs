using Dapper;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.Database;
using PhucLong.Interface.Central.Models.Partner.ESTELLA;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using VCM.Common.Helpers;

namespace PhucLong.Interface.Central.AppService.Partner
{
    public class ESTELLAService
    {
        private IConfiguration _configuration;
        private DapperCentral _dapperContext;
        public ESTELLAService
           (
               IConfiguration config
           )
        {
            _configuration = config;
            _dapperContext = new DapperCentral(_configuration);
        }

        public bool SalesFileGeneration(string pathLocal, string procedure)
        {
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
            conn.Open();
            try
            {
                var dataSales = conn.Query<Sales_ESTELLA>(@"EXEC " + procedure).ToList();
                FileHelper.WriteLogs("===> EXEC " + procedure + " ===> result: " + dataSales.Count.ToString());
                if(dataSales.Count > 0) 
                {
                    var lstStore = dataSales.Select(x => new
                    {
                        x.MachineID, x.OrderDate
                    }).GroupBy(x => new { x.MachineID, x.OrderDate })
                            .Select(x =>
                            {
                                var temp = x.OrderBy(o => o.OrderDate).FirstOrDefault();
                                return new
                                {
                                    x.Key.MachineID, x.Key.OrderDate
                                };
                            }).OrderBy(x => x.OrderDate).ToList();

                    if(lstStore.Count > 0)
                    {
                        foreach(var store in lstStore)
                        {
                            var orderDate = store.OrderDate.ToString();
                            var fileName = "H" + store.MachineID.ToString() + "_" + orderDate + ".txt";
                            var dataSalesStore = dataSales.Where(x => x.MachineID == store.MachineID && x.OrderDate == store.OrderDate).ToList();
                            List<string> dataString = new List<String>();
                            foreach (var item in dataSalesStore)
                            {
                                dataString.Add(
                                    item.MachineID
                                    + "|" + item.BatchID.ToString()
                                    + "|" + item.Date.ToString()
                                    + "|" + item.Hour.ToString()
                                    + "|" + item.ReceiptCount.ToString()
                                    + "|" + item.GTO.ToString()
                                    + "|" + item.GST.ToString()
                                    + "|" + item.Discount.ToString()
                                    + "|" + item.ServiceCharge.ToString()
                                    + "|" + item.NoOfPax.ToString()
                                    + "|" + item.Cash.ToString()
                                    + "|" + item.DebitCard.ToString()
                                    + "|" + item.VisaCard.ToString()
                                    + "|" + item.MasterCard.ToString()
                                    + "|" + item.Amex.ToString()
                                    + "|" + item.Voucher.ToString()
                                    + "|" + item.OthersAmount.ToString()
                                    + "|" + item.Registered.ToString()
                                    );
                            }

                            if (File.Exists(pathLocal + fileName))
                            {
                                File.Delete(pathLocal + fileName);
                            }

                            if(FileHelper.WriteTxt(pathLocal, fileName, dataString.ToArray()))
                            {
                                conn.Execute("UPDATE Partner_ESTELLA SET UpdateFlg = 'Y' WHERE MachineID = " + store.MachineID + " AND [OrderDate] = '" + orderDate + "' AND BatchID = " + dataSalesStore.FirstOrDefault().BatchID);
                            }

                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("SalesFileGeneration Exception:" + ex.Message.ToString());
                return false;
            }
        }

    }
}
