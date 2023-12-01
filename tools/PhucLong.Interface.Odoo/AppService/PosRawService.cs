using Dapper;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Odoo.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using VCM.Common.Helpers;
using VCM.Shared.Entity.PhucLong;

namespace PhucLong.Interface.Odoo.AppService
{
    public class PosRawService
    {
        private IConfiguration _config;
        private readonly ConndbNpgsql _dbOdoo;
        public PosRawService
        (
         IConfiguration config
        )
        {
            _config = config;
            _dbOdoo = new ConndbNpgsql(_config);
        }
        public void DeletePosRaw(IDbConnection conn)
        {
            try
            {
                using var transaction = conn.BeginTransaction();
                try
                {
                    int rows = conn.Execute(@" DELETE FROM public.pos_raw WHERE  CAST(crt_date as DATE) < '" + DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd") + "'; ");
                    FileHelper.WriteLogs("===> public.pos_raw DELETED: " + rows.ToString() + " rows");

                    int pos_staging = conn.Execute(@" DELETE FROM public.pos_staging WHERE  CAST(crt_date as DATE) < '" + DateTime.Now.AddDays(-35).ToString("yyyy-MM-dd") + "'; ");
                    FileHelper.WriteLogs("===> public.pos_raw DELETED: " + pos_staging.ToString() + " rows");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    FileHelper.WriteLogs("DeletePosRaw Rollback Exception: " + ex.Message.ToString());
                }
               
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("DeletePosRaw Exception: " + ex.Message);
            }
        }
        public void SavePosRawOdoo(IDbConnection conn, List<Pos_Raw> trans, string rawType, string pathLocal, bool isHistoric)
        {
            using (var transaction = conn.BeginTransaction())
            {
                int affecaffectedRows = 0;
                if (isHistoric)
                {
                    var queryIns = @"INSERT INTO public.pos_raw (order_id, location_id, is_sending, raw_data, crt_date) VALUES (@order_id, @location_id, @is_sending, CAST(@raw_data AS json), now());";
                    affecaffectedRows = conn.Execute(queryIns, trans, transaction);
                    transaction.Commit();
                    FileHelper.WriteLogs("===> Saved to public.pos_raw successfully: " + affecaffectedRows.ToString());
                }
            }

            FileHelper.WriteLogs("===> Strat create file");
            int fileNumber = 0;
            foreach (var tran in trans)
            {
                string fileName = tran.order_id.ToString();
                if (FileHelper.CreateFileMaster(fileName, rawType, pathLocal, tran.raw_data))
                {
                    tran.is_sending = true;
                    fileNumber++;
                }
                Thread.Sleep(50);
                Console.WriteLine("created: " + fileName);
            }
            FileHelper.WriteLogs("Created: " + fileNumber.ToString() + " file transaction");
        }
    }

}
