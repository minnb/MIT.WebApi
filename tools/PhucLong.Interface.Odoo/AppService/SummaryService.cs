using Dapper;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Odoo.Database;
using PhucLong.Interface.Odoo.Models.RECONCILE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Odoo.AppService
{
    public class SummaryService
    {
        private IConfiguration _config;
        private readonly ConndbNpgsql _dbOdoo;
        private PosRawService posRawService;
        public SummaryService
            (
             IConfiguration config
            )
        {
            _config = config;
            _dbOdoo = new ConndbNpgsql(_config);
            posRawService = new PosRawService(_config);
        }
        public void SaveSalesReconcileOdoo(string connectString, string pathLocal, string subSet)
        {
            string typeSummary = "SALES-" + subSet.ToString();
            string summary_date = _config["AppSetting:EntryDate"].ToString();
            if (string.IsNullOrEmpty(summary_date))
            {
                summary_date = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            string logfile = pathLocal + typeSummary + "_" + summary_date + ".txt";
            FileHelper.WriteLogs("===> START: " + typeSummary + " | Date: " + summary_date);
            if (File.Exists(logfile))
            {
                FileHelper.WriteLogs("Exists " + logfile);
            }
            else
            {
                var dataOdoo = SalesReconcileOdoo(connectString, summary_date);
                if (dataOdoo.Count > 0)
                {
                    ConndbCentral _dbContext = new ConndbCentral(_config);
                    using var conn = _dbContext.ConnCentralPhucLong;
                    conn.Open();
                    var checkPaymentSummary = conn.Query<Summary>(@"SELECT * FROM [Summary] WHERE [Code] = '" + typeSummary + "' AND EntryDate = '" + summary_date + "';").ToList();

                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        List<Summary> dataSum = new List<Summary>();
                        if (checkPaymentSummary.Count > 0)
                        {
                            conn.Execute(@"DELETE [Summary] WHERE [Code] = @Code AND EntryDate = @EntryDate;", checkPaymentSummary, transaction);
                        }
                        foreach (var item in dataOdoo)
                        {
                            dataSum.Add(new Summary()
                            {
                                Code = typeSummary,
                                EntryDate = item.EntryDate.ToString("yyyyMMdd"), //DateTime.ParseExact(payment_date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                TypeCode = item.StoreNo,
                                TypeName = "",
                                TotalBill = item.TotalBill,
                                TotalAmount = item.TotalAmount,
                                PaymentAmount = item.PaymentAmount,
                                DiscountAmount = item.TotalAmount - item.PaymentAmount,
                                UpdateFlg = "N",
                                CrtDate = DateTime.Now,
                                Id = Guid.NewGuid()
                            });
                        }
                        var queryInsert = @"INSERT INTO [dbo].[Summary]([Code],[EntryDate],[TypeCode],[TypeName],[TotalBill],[TotalAmount],PaymentAmount,DiscountAmount,[UpdateFlg],[CrtDate],[Id])
                                                               VALUES(@Code, @EntryDate, @TypeCode, @TypeName, @TotalBill, @TotalAmount,@PaymentAmount,@DiscountAmount, @UpdateFlg, @CrtDate, newId())";
                        conn.Execute(queryInsert, dataSum, transaction);

                        transaction.Commit();
                        File.Create(logfile).Dispose();
                        FileHelper.WriteLogs("===>DONE: SavePaymentReconcileOdoo");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ExceptionHelper.WriteExptionError("SavePaymentReconcileOdoo conn.BeginTransaction Exception", ex);
                    }
                }
            }
        }

        public void SavePaymentReconcileOdoo(string connectString, string pathLocal, string subSet)
        {
            string typeSummary = "PAYMENT-" + subSet.ToString();
            string summary_date = _config["AppSetting:EntryDate"].ToString();
            if (string.IsNullOrEmpty(summary_date))
            {
                summary_date = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            string logfile = pathLocal + typeSummary + "_" + summary_date + ".txt";
            FileHelper.WriteLogs("===> START: "+ typeSummary + " | Date: " + summary_date);
            if (File.Exists(logfile))
            {
                FileHelper.WriteLogs("Exists " +  logfile);
            }
            else
            {
                var paymentOdoo = PaymentReconcileOdoo(connectString, summary_date);
                if (paymentOdoo.Count > 0)
                {
                    ConndbCentral _dbContext = new ConndbCentral(_config);
                    using var conn = _dbContext.ConnCentralPhucLong;
                    conn.Open();
                    var checkPaymentSummary = conn.Query<Summary>(@"SELECT * FROM [Summary] WHERE [Code] = '" + typeSummary + "' AND EntryDate = '" + summary_date + "';").ToList();

                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        List<Summary> dataSum = new List<Summary>();
                        if (checkPaymentSummary.Count > 0)
                        {
                            conn.Execute(@"DELETE [Summary] WHERE [Code] = @Code AND EntryDate = @EntryDate;", checkPaymentSummary, transaction);
                        }
                        foreach (var item in paymentOdoo)
                        {
                            dataSum.Add(new Summary()
                            {
                                Code = typeSummary,
                                EntryDate = item.PaymentDate.ToString("yyyyMMdd"), //DateTime.ParseExact(payment_date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                TypeCode = item.PaymentMethod,
                                TypeName = item.PaymentName,
                                TotalBill = 0,
                                TotalAmount = item.PaymentAmount,
                                PaymentAmount = item.PaymentAmount,
                                DiscountAmount = 0,
                                UpdateFlg = "N",
                                CrtDate = DateTime.Now,
                                Id = Guid.NewGuid()
                            });
                        }
                        var queryInsert = @"INSERT INTO [dbo].[Summary]([Code],[EntryDate],[TypeCode],[TypeName],[TotalBill],[TotalAmount],PaymentAmount,DiscountAmount,[UpdateFlg],[CrtDate],[Id])
                                                               VALUES(@Code, @EntryDate, @TypeCode, @TypeName, @TotalBill, @TotalAmount,@PaymentAmount,@DiscountAmount, @UpdateFlg, @CrtDate, newId())";
                        conn.Execute(queryInsert, dataSum, transaction);

                        transaction.Commit();
                        File.Create(logfile).Dispose();
                        FileHelper.WriteLogs("===>DONE: SavePaymentReconcileOdoo");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ExceptionHelper.WriteExptionError("SavePaymentReconcileOdoo conn.BeginTransaction Exception", ex);
                    }
                }
            }
        }
        private List<PaymentReconcileModel> PaymentReconcileOdoo(string connectString, string payment_date)
        {
            try
            {
                using IDbConnection conn = _dbOdoo.GetConndbNpgsql(connectString);
                conn.Open();

                string querySumPayment = @"select cast(a.date_order::timestamp AT TIME ZONE 'UTC' as date) as PaymentDate, coalesce(p.sap_method, 'EMPTY') as PaymentMethod, p.name PaymentName, sum(b.amount) PaymentAmount
                                        from pos_order a
                                        inner join pos_payment b on a.id = b.pos_order_id and a.state = 'paid'
                                        inner join pos_payment_method p on p.id = b.payment_method_id
                                        where cast(a.date_order::timestamp AT TIME ZONE 'UTC' as date) = '"+ payment_date + @"'
                                        group by cast(a.date_order::timestamp AT TIME ZONE 'UTC' as date), p.name, p.sap_method
                                        order by p.sap_method;";

                var data = conn.Query<PaymentReconcileModel>(querySumPayment).ToList();
                return data;

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("PaymentReconcileOdoo Exception: " + ex.Message.ToString());
                return null;
            }
        }
        private List<SalesReconcileModel> SalesReconcileOdoo(string connectString, string order_date)
        {
            try
            {
                using IDbConnection conn = _dbOdoo.GetConndbNpgsql(connectString);
                conn.Open();
                string query = @"select s.code as StoreNo, cast(a.date_order::timestamp AT TIME ZONE 'UTC' as date) as EntryDate, c.TotalBill, sum(b.price_unit * b.qty) as TotalAmount, c.amount_paid AS PaymentAmount
                                            from pos_order a
                                            inner join pos_order_line b on a.id = b.order_id and cast(a.date_order::timestamp AT TIME ZONE 'UTC' as date) = '" + order_date + @"'
                                            inner join stock_warehouse s on s.id = a.warehouse_id
                                            inner join (select s.code, a.warehouse_id, count(a.name) as TotalBill, sum(a.amount_total) amount_paid
			                                            from pos_order a
			                                            inner join stock_warehouse s on s.id = a.warehouse_id
			                                            where a.state = 'paid' and cast(a.date_order::timestamp AT TIME ZONE 'UTC' as date) = '" + order_date  + @"'
			                                            group by s.code, a.warehouse_id) c on c.code = s.code and c.warehouse_id = a.warehouse_id
                                            where a.state = 'paid'
                                            group by s.code, cast(a.date_order::timestamp AT TIME ZONE 'UTC' as date), c.TotalBill, c.amount_paid;";

                var data = conn.Query<SalesReconcileModel>(query).ToList();
                return data;

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("PaymentReconcileOdoo Exception: " + ex.Message.ToString());
                return null;
            }
        }
    }
}
