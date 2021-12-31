using AutoMapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Odoo.Database;
using PhucLong.Interface.Odoo.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.Entity.PhucLong;

namespace PhucLong.Interface.Odoo.AppService
{
    public class ArchiveDataService
    {
        private readonly IConfiguration _config;
        private readonly ConndbNpgsql _dbOdoo;
        private readonly ConndbRedis _conndbRedis;
        public ArchiveDataService
            (
             IConfiguration config
            )
        {
            _config = config;
            _dbOdoo = new ConndbNpgsql(_config);
            _conndbRedis = new ConndbRedis(_config);
        }

        public async Task<int> ArchivePosStagingAsync(string connectString, string back_date)
        {
            try
            {
                using IDbConnection conn = _dbOdoo.GetConndbNpgsql(connectString);
                conn.Open();
                Console.WriteLine("Archived date: " + back_date + " processing....");
                FileHelper.WriteLogs("Archived date: " + back_date + " processing....");
                var result = await conn.QueryAsync(ArchivePosStagingQuery.ArchivePosStaging(back_date)).ConfigureAwait(false);
                if(result != null && result.ToList().Count > 0)
                {
                    FileHelper.WriteLogs("===> START: " + DateTime.Now.ToString("HH:mm:ss"));
                    var redis = _conndbRedis.GetRedisServer();
                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        foreach(var item in result.ToList())
                        {
                            if (item.is_payment)
                            {
                                var dataInser = new Pos_Staging()
                                {
                                    order_id = item.order_id,
                                    location_id = item.location_id,
                                    pos_reference = item.pos_reference,
                                    is_display = item.is_display,
                                    is_payment = item.is_payment,
                                    state = item.state,
                                    raw_data = item.raw_data,
                                    crt_date = item.crt_date,
                                    upd_date = item.upd_date,
                                };

                                var queryIns = @"INSERT INTO public.pos_staging_history (order_id, location_id, is_display, is_payment, state, pos_reference, raw_data, crt_date, upd_date) 
                                                                VALUES (@order_id, @location_id, @is_display, @is_payment, @state, @pos_reference, CAST(@raw_data AS json), @crt_date, @upd_date);";
                                await conn.ExecuteAsync(queryIns, dataInser, transaction);
                            }
                            if(redis.KeyDelete(item.location_id + "." + item.pos_reference))
                            {
                                FileHelper.WriteLogs("KeyDelete: " + item.location_id + "." + item.pos_reference);
                            }
                        }

                        var queryDel = @"DELETE FROM public.pos_staging WHERE order_id = @order_id AND location_id = @location_id";
                        await conn.ExecuteAsync(queryDel, result.ToList(), transaction);

                        await conn.ExecuteAsync(@"DELETE FROM public.pos_staging_history WHERE Cast(crt_date::timestamp AT TIME ZONE 'UTC' as date) <= '" + DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd") + @"'", transaction);

                        transaction.Commit();
                        FileHelper.WriteLogs("===> processed: " + result.ToList().Count.ToString());
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        FileHelper.WriteLogs("Rollback Exception: " + ex.Message.ToString());
                    }
                    FileHelper.WriteLogs("===> END: " + DateTime.Now.ToString("HH:mm:ss"));
                }

                return result != null ? result.ToList().Count : 0;
            }
            catch(Exception ex)
            {
                FileHelper.WriteLogs("ArchivePosStagingAsync: " + ex.Message.ToString());
                return 0;
            }
        }
    }
}
