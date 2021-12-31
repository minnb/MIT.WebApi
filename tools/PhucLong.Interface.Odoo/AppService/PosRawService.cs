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

        public void SavePosRawOdoo(IDbConnection conn, List<Pos_Raw> trans, string rawType, string pathLocal, bool isHistoric)
        {
            int affecaffectedRows = 0;
            int fileNumber = 0;

            using (var transaction = conn.BeginTransaction())
            {
                if (isHistoric)
                {
                    var queryIns = @"INSERT INTO public.pos_raw (order_id, location_id, is_sending, raw_data, crt_date) VALUES (@order_id, @location_id, @is_sending, CAST(@raw_data AS json), now());";
                    affecaffectedRows = conn.Execute(queryIns, trans, transaction);
                    transaction.Commit();

                    FileHelper.WriteLogs("AffecaffectedRows: " + affecaffectedRows.ToString());
                    FileHelper.WriteLogs("Saved: " + trans.Count.ToString() + " record");
                }
            }

            FileHelper.WriteLogs("Strat create file");
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
            FileHelper.WriteLogs("Saved to public.pos_raw successfully: " + affecaffectedRows.ToString());
        }
    }

}
