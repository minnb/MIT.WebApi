using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Central.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Tools.Common.Models.Loyalty;
using VCM.Common.Helpers;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Central.AppService
{
    public class LoyaltyService
    {
        private IConfiguration _configuration;
        private DapperCentral _dapperContext;

        public LoyaltyService
          (
              IConfiguration config
          )
        {
            _configuration = config;
            _dapperContext = new DapperCentral(_configuration);
        }

        public void GetDataRawLoyaltyAsync(string storeProcedure)
        {
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
            conn.Open();
            try
            {
                WebApiService webApiService = new WebApiService(_configuration);
                var lstWebApi = webApiService.GetWebApiInfo(conn, "LOY");
                var apiLoy = lstWebApi.WebRoute.Where(x => x.Name == "MemberPointsEarn").FirstOrDefault();
                if (lstWebApi != null && apiLoy != null)
                {
                    string queryLoy = @"EXEC " + storeProcedure;
                    var transRaw = conn.Query<SalesLoyalty>(queryLoy).ToList();
                    Console.WriteLine("SELECTED: " + transRaw.Count.ToString());

                    if (transRaw.Count > 0)
                    {
                        foreach (var item in transRaw)
                        {
                            if (!string.IsNullOrEmpty(item.Phone) && RegularHelper.ValidatePhoneNumber(item.Phone, false))
                            {
                                var storeId = string.Empty;
                                if (!string.IsNullOrEmpty(item.PosNo))
                                {
                                    storeId = item.PosNo.Substring(0, item.PosNo.Length - 2);
                                }

                                var loyDto = new LoyaltyDto()
                                {
                                    QRCode = "",
                                    CardNumber = RegularHelper.RemoveNonNumeric(item.Phone),
                                    MerchantId = storeId,
                                    TerminalId = item.PosNo,
                                    InvoiceNo = item.OrderNo,
                                    OrderNo = ((long)DateTimeOffset.Now.ToUnixTimeMilliseconds()).ToString(), //item.RefNo,
                                    SpendPoints = 0,
                                    BillAmount = item.AmountInclVAT,
                                    OrderAmount = item.AmountInclVAT,
                                    IsOffline = true,
                                    VirtualCard = ""
                                };

                                Console.WriteLine("Call {0}", item.OrderNo);
                                RspLoyalty rspLoyalty = new RspLoyalty();

                                rspLoyalty = new RspLoyalty()
                                {
                                    Status = 200,
                                    Message = "OK",
                                    Data = new DataLoyalty()
                                    {
                                        CurrentRate = 1,
                                        IsOfflineVinID = false,
                                        PointRedeem = 0,
                                        PointEarn = (int)Math.Round(item.AmountInclVAT / (100), 0)
                                    }
                                };

                                //LoyaltyEarn(loyDto, lstWebApi.Host, apiLoy.Route, ref rspLoyalty);

                                if (rspLoyalty.Status == 200)
                                {
                                    using (IDbTransaction transaction = conn.BeginTransaction())
                                    {
                                        try
                                        {
                                            if (!PointsDistribution(conn, transaction, item.OrderNo, item.AmountInclVAT, rspLoyalty.Data.PointEarn))
                                            {
                                                _dapperContext.StepProcessLoyalty(conn, transaction, new TransHeaderStepLoy() { OrderNo = item.OrderNo, StepProcess = 9, MemberPointsEarn = 0 });
                                                //FileHelper.WriteLogs("OrderNo: " + item.RefNo + " @Response:" + JsonConvert.SerializeObject(rspLoyalty));
                                            }
                                            else
                                            {
                                                FileHelper.WriteLogs("OrderNo: " + item.RefNo + " @Response:" + JsonConvert.SerializeObject(rspLoyalty));
                                            }

                                            transaction.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            transaction.Rollback();
                                            FileHelper.WriteLogs("BeginTransaction Exception:" + ex.Message.ToString());
                                        }
                                    }
                                    
                                }
                            }
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                FileHelper.WriteLogs("IDbConnection Exception:" + ex.Message.ToString());
            }
        }

        public bool PointsDistribution(IDbConnection conn, IDbTransaction transaction, string orderNo, decimal amountInclVAT, int PointEarn)
        {
            var transLine = conn.Query<TransLine>(@"SELECT * FROM TransLine (NOLOCK) WHERE OrderNo = @OrderNo;", new { OrderNo  = orderNo }, transaction).ToList();
            var discountEntry = conn.Query<TransDiscountEntry>(@"SELECT * FROM TransDiscountEntry (NOLOCK) WHERE OrderNo = @OrderNo ;", new { OrderNo = orderNo }, transaction).ToList();

            if (transLine.Count > 0 && amountInclVAT > 0)
            {
                var transSale = transLine.Where(x => x.LineType == 0 && x.LineAmountIncVAT > 0).OrderByDescending(x => x.LineAmountIncVAT).ToList();
                int totalLine = transSale.Count();
                decimal tempPoint = 0;
                int count = 0;
                List<TransLinePoint> transLinePoints = new List<TransLinePoint>();
                foreach (var item in transSale)
                {
                    count++;
                    decimal disPoint = Math.Round((item.LineAmountIncVAT * PointEarn) / amountInclVAT, 0);
                    tempPoint += disPoint;

                    if (count == totalLine)
                    {
                        if (tempPoint != PointEarn)
                        {
                            disPoint -= (tempPoint - PointEarn);
                        }
                    }

                    transLinePoints.Add(new TransLinePoint()
                    {
                        OrderNo = item.OrderNo,
                        LineNo = item.LineNo,
                        MemberPointsEarn = disPoint,
                        LineAmountInclVAT = item.LineAmountIncVAT,
                        ItemNo = item.ItemNo
                    });
                }

                if (transLinePoints.Count > 0)
                {
                    string upLine = @"UPDATE TransLine SET MemberPointsEarn = @MemberPointsEarn WHERE OrderNo = @OrderNo AND [LineNo] = @LineNo;";
                    conn.Execute(upLine, transLinePoints, transaction);

                    //Insert Discount
                    int maxLineNo = discountEntry.Count();
                    List<TransDiscountEntry> transDiscountEntries = new List<TransDiscountEntry>();
                    foreach (var linePoint in transLinePoints)
                    {
                        maxLineNo++;
                        decimal pointAmounExclVAT = MathHelper.CalcNetAmount(linePoint.MemberPointsEarn, 10);
                        decimal pointAmountVAT = linePoint.MemberPointsEarn - pointAmounExclVAT;
                        transDiscountEntries.Add(new TransDiscountEntry()
                        {
                            OrderNo = linePoint.OrderNo,
                            OrderId = linePoint.LineNo,
                            LineId = linePoint.LineNo,
                            LineNo = maxLineNo,
                            OrderLineNo = linePoint.LineNo,
                            OfferNo = "",
                            OfferType = "PLD3",
                            Quantity = pointAmounExclVAT,
                            DiscountType = 0,
                            DiscountAmount = linePoint.LineAmountInclVAT,
                            ParentLineNo = linePoint.LineNo,
                            ItemNo = linePoint.ItemNo,
                            LineGroup = "PLD3"
                        });

                        maxLineNo++;
                        transDiscountEntries.Add(new TransDiscountEntry()
                        {
                            OrderNo = linePoint.OrderNo,
                            OrderId = linePoint.LineNo,
                            LineId = linePoint.LineNo,
                            LineNo = maxLineNo,
                            OrderLineNo = linePoint.LineNo,
                            OfferNo = "",
                            OfferType = "PLD4",
                            Quantity = pointAmountVAT,
                            DiscountType = 0,
                            DiscountAmount = linePoint.LineAmountInclVAT,
                            ParentLineNo = linePoint.LineNo,
                            ItemNo = linePoint.ItemNo,
                            LineGroup = "PLD4"
                        });
                    }
                    _dapperContext.InsTransDiscountEntry(conn, transaction, transDiscountEntries);
                }

                _dapperContext.StepProcessLoyalty(conn, transaction, new TransHeaderStepLoy() { OrderNo = orderNo, StepProcess = 1, MemberPointsEarn = PointEarn });
                
                return true;
            }
            else
            {
                return false;
            }
        }
        public void LoyaltyEarn(LoyaltyDto loy, string url, string route, ref RspLoyalty rspLoyalty)
        {
            var param = route + @"?QRCode=";
            param += @"&CardNumber=" + loy.CardNumber;
            param += @"&MerchantId=" + loy.MerchantId;
            param += @"&TerminalId=" + loy.TerminalId;
            param += @"&InvoiceNo=" + loy.OrderNo;
            param += @"&SpendPoints=" + loy.SpendPoints;
            param += @"&BillAmount=" + loy.BillAmount;
            param += @"&OrderNo=" + loy.OrderNo;
            param += @"&IsOffline=False";
            param += @"&OrderAmount=" + loy.OrderAmount;
            param += @"&VirtualCard=";

            RestShapHelper apiHelper = new RestShapHelper(
                url,
                param,
                "GET",
                null,
                null,
                null,
                30000,
                null,
                0
            );

            string mess_errors = string.Empty;
            int statusRsp = 200;
            var result = apiHelper.InteractWithApi(ref statusRsp, ref mess_errors);

            RspLoyalty rsp = JsonConvert.DeserializeObject<RspLoyalty>(result);
            FileHelper.WriteLogs(result);

            if (rsp != null)
            {
                if (rsp.Status == 200 || rsp.Status == 409)
                {
                    if (rsp.Status == 200)
                    {
                        rspLoyalty = rsp;
                    }
                    else
                    {
                        FileHelper.WriteLogs("OrderNo: " + loy.OrderNo + " ==> " + JsonConvert.SerializeObject(rsp));
                    }
                }
            }
            else
            {
                FileHelper.WriteLogs(JsonConvert.SerializeObject(rsp));
            }
        }
      
    }
}


//84559151598
//84559384826
//84551130807
//84551130808
//84551130804