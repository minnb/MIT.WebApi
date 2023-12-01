using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Odoo.Database;
using PhucLong.Interface.Odoo.Enum;
using PhucLong.Interface.Odoo.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.Entity.PhucLong;
using VCM.Shared.Entity.PhucLong.Dtos;

namespace PhucLong.Interface.Odoo.AppService
{
    public class PosOrderService
    {
        private IConfiguration _config;
        private readonly ConndbNpgsql _dbOdoo;
        private PosRawService posRawService;
        public PosOrderService
            (
             IConfiguration config
            )
        {
            _config = config;
            _dbOdoo = new ConndbNpgsql(_config);
            posRawService = new PosRawService(_config);
        }

        public void GetDataOdoo(string connectString, string date_order, int _limit, string pathLocal, bool isHistoric, int id = 0)
        {
            try
            {
                FileHelper.WriteLogs(connectString);
                using IDbConnection conn = _dbOdoo.GetConndbNpgsql(connectString);
                conn.Open();
                Console.WriteLine("date_order {0} top {1} processing....", date_order.ToString(), _limit.ToString());

                var dataTransHeader = conn.Query<Pos_Order>(PosOrderQuery.StrQueryTransHeader(date_order, _limit, isHistoric, id)).ToList();

                FileHelper.WriteLogs("===> Date: " + date_order.ToString() + " | Selected: " + dataTransHeader.Count.ToString());
                if (dataTransHeader.Count > 0)
                {
                    var lstOrderId = new List<int>();
                    foreach (var o in dataTransHeader)
                    {
                        lstOrderId.Add(o.id);
                    }

                    var transLine = conn.Query<Pos_Order_Line>(PosOrderQuery.StrQueryTransLine(), new { order_id = lstOrderId }).ToList();
                    var transPayment = conn.Query<Pos_Payment>(PosOrderQuery.StrQueryTransPaymentEntry(), new { order_id = lstOrderId }).ToList();
                    var transLineOptions = conn.Query<Pos_Order_Line_Option>(PosOrderQuery.StrQueryTransLineOptions(), new { line_id = transLine.Select(x => x.id).ToList() }).ToList();

                    List<Pos_Raw> trans = new List<Pos_Raw>();
                    foreach (var item in dataTransHeader)
                    {
                        var dataLines = transLine.Where(x => x.order_id == item.id).ToList();
                        var listLineOp = dataLines.Select(x => x.id).ToList();

                        trans.Add(new Pos_Raw()
                        {
                            order_id = item.id,
                            location_id = item.location_id,
                            is_sending = false,
                            raw_data = JsonConvert.SerializeObject(new PosRawDto()
                            {
                                TransHeader = item,
                                TransLine = dataLines,
                                TransPaymentEntry = transPayment.Where(x => x.pos_order_id == item.id).ToList(),
                                PosOrderLineOption = transLineOptions.Where(x => listLineOp.Contains(x.line_id)).ToList()
                            })
                        });
                    }

                    if (trans.Count > 0)
                    {
                        posRawService.SavePosRawOdoo(conn, trans, "PLG", pathLocal, isHistoric);
                    }
                }


                if (DateTime.Now.ToString("HH") == "09")
                {
                    FileHelper.WriteLogs("===> Archive data raw");
                    PosRawService posRawService = new PosRawService(_config);
                    posRawService.DeletePosRaw(conn);
                }
            }
            catch(Exception ex)
            {
                FileHelper.WriteLogs("PLG Sales Exception: " + ex.Message.ToString());
            }
        }
        public void Get_pos_order_request_vat(string connectString, string date_orde, string pathLocal)
        {
            int location_id = (int)RawTypeOdooEnum.VATInfo;
            using IDbConnection conn = _dbOdoo.GetConndbNpgsql(connectString);
            conn.Open();
            try
            {
                string querySQL = @"SELECT id, name, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, state, location_id, warehouse_id, 
                                            cast(create_date::timestamp AT TIME ZONE 'UTC' as timestamp) create_date,
	                                        cast(write_date::timestamp AT TIME ZONE 'UTC' as timestamp) write_date, note, note_label,
											invoice_name, invoice_vat, invoice_address, invoice_email, invoice_contact, invoice_note, invoice_request
                                        FROM public.pos_order s
                                        WHERE s.invoice_request = true
                                              AND EXISTS (SELECT 1 FROM public.pos_raw r WHERE s.id = r.order_id AND s.location_id = r.location_id)
                                              AND NOT EXISTS (SELECT 1 FROM public.pos_raw p WHERE p.location_id = " + location_id + @" AND s.id = p.order_id) 
                                              AND Cast(s.date_order::timestamp AT TIME ZONE 'UTC' as date) >= '" + date_orde + @"' ORDER BY s.date_order LIMIT(100) ;"; 
                var dataQuery = conn.Query<PosRequestVatDto>(querySQL).ToList();
                if (dataQuery.Count > 0)
                {
                    List<Pos_Raw> trans = new List<Pos_Raw>();

                    foreach (var item in dataQuery)
                    {
                        trans.Add(new Pos_Raw()
                        {
                            order_id = item.id,
                            location_id = location_id,
                            is_sending = false,
                            raw_data = JsonConvert.SerializeObject(item)
                        });
                    }

                    if (trans.Count > 0)
                    {
                        //posRawService.SavePosRawOdoo(conn, trans, "POS_VAT", pathLocal, true);
                        int fileNumber = 0;
                        int affecaffectedRows = 0;
                        foreach (var tran in trans)
                        {
                            string fileName = tran.order_id.ToString();
                            if (FileHelper.CreateFileMaster(fileName, "POS_VAT", pathLocal, tran.raw_data))
                            {
                                tran.is_sending = true;
                                fileNumber++;
                            }
                            Thread.Sleep(50);
                            Console.WriteLine("created: " + fileName);
                        }
                        FileHelper.WriteLogs("Created: " + fileNumber.ToString() + " file transaction");

                        if (DateTime.Now.ToString("HH") == "23")
                        {
                            using var transaction = conn.BeginTransaction();
                            try
                            {
                                var queryIns = @"INSERT INTO public.pos_raw (order_id, location_id, is_sending, raw_data, crt_date) VALUES (@order_id, @location_id, @is_sending, CAST(@raw_data AS json), now());";
                                affecaffectedRows = conn.Execute(queryIns, trans, transaction);
                                transaction.Commit();
                            }
                            catch(Exception ex)
                            {
                                transaction.Rollback();
                                FileHelper.WriteLogs("Get_pos_order_request_vat Exception: " + ex.Message.ToString());
                            }

                            FileHelper.WriteLogs("AffecaffectedRows: " + affecaffectedRows.ToString());
                            FileHelper.WriteLogs("Saved to public.pos_raw successfully: " + trans.Count.ToString() + " record");
                        }                           
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("Get_pos_order_request_vat", ex);
            }
        }
        public void Get_pos_order_cancel(string connectString, string date_orde, string pathLocal)
        {
            int location_id = (int)RawTypeOdooEnum.OrderCancel;
            using IDbConnection conn = _dbOdoo.GetConndbNpgsql(connectString);
            conn.Open();
            try
            {
                string querySQL = @"SELECT id, name, cast(date_order::timestamp AT TIME ZONE 'UTC' as date) date_order, state, location_id, warehouse_id,
                                            cast(date_last_order::timestamp AT TIME ZONE 'UTC' as timestamp) date_last_order,
	                                        cast(write_date::timestamp AT TIME ZONE 'UTC' as timestamp) write_date,note, note_label
                                        FROM public.pos_order s
                                        WHERE s.state = 'cancel' 
                                              AND EXISTS (SELECT 1 FROM public.pos_raw r WHERE s.id = r.order_id AND s.location_id = r.location_id)
                                              AND NOT EXISTS (SELECT 1 FROM public.pos_raw p WHERE p.location_id = " + location_id + @" AND s.id = p.order_id) 
                                              AND Cast(s.date_order::timestamp AT TIME ZONE 'UTC' as date) >= '" + date_orde + @"' ORDER BY s.date_order LIMIT(100) ;";
                var dataQuery = conn.Query<PosOrderCancelDto>(querySQL).ToList();
                if (dataQuery.Count > 0)
                {
                    List<Pos_Raw> trans = new List<Pos_Raw>();
                    foreach (var item in dataQuery)
                    {
                        trans.Add(new Pos_Raw()
                        {
                            order_id = item.id,
                            location_id = location_id,
                            is_sending = false,
                            raw_data = JsonConvert.SerializeObject(item)
                        });
                    }

                    if (trans.Count > 0)
                    {
                        posRawService.SavePosRawOdoo(conn, trans, "POS_ORDER_CANCEL", pathLocal, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("Get_pos_order_cancel", ex);
            }
        }
    }
}
