using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using VCM.Shared.Entity.Central;

namespace PhucLong.Interface.Central.Database
{
    public class DapperCentral
    {
        private readonly IConfiguration _configuration;
        public DapperCentral(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection ConnCentralPhucLong
        {
            get
            {
                return new SqlConnection(_configuration["ConnectionStrings:PhucLongStaging"]);
            }
        }

        public void InsTransDiscountEntry(IDbConnection conn, IDbTransaction transaction, List<TransDiscountEntry> transDiscountEntry)
        {
            string insDiscount = @"INSERT INTO [dbo].[TransDiscountEntry]
                                                            ([OrderNo],[OrderId],[LineId],[LineNo],[OrderLineNo],[ParentLineNo],[ItemNo],[OfferNo],[OfferType],[DiscountType],[Quantity],[DiscountAmount],[LineGroup])
                                                        VALUES (@OrderNo,@OrderId,@LineId,@LineNo,@OrderLineNo,@ParentLineNo,@ItemNo,@OfferNo,@OfferType,@DiscountType,@Quantity,@DiscountAmount,@LineGroup)";
            conn.Execute(insDiscount, transDiscountEntry, transaction);
        }
        public void StepProcessLoyalty(IDbConnection conn, IDbTransaction transaction, TransHeaderStepLoy transHeaderStep)
        {
            string query = @"UPDATE TransHeader SET StepProcess = @StepProcess, MemberPointsEarn = @MemberPointsEarn WHERE OrderNo = @OrderNo";
            conn.Execute(query, transHeaderStep, transaction);
        }

    }
    public class TransHeaderStepLoy
    {
        public string OrderNo { get; set; }
        public int StepProcess { get; set; }
        public decimal MemberPointsEarn { get; set; }
    }
}
