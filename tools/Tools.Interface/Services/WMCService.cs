using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Tools.Interface.Database;
using VCM.Common.Database;
using VCM.Common.Helpers;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Dtos.WinMoney;
using VCM.Shared.Entity.Central;

namespace Tools.Interface.Services
{
    public class WMCService
    {
        private readonly InterfaceEntry _interfaceEntry;
        private DapperContext _dapperContext;
        public WMCService
            (
            InterfaceEntry interfaceEntry
            ) 
        {
            _interfaceEntry = interfaceEntry;
        }

        public void ImportWinMoneyTrans(InterfaceEntry interfaceEntry)
        {
            try
            {
                bool flag = true;
                var lstFile = FileHelper.GetFileFromDir(interfaceEntry.LocalPath, interfaceEntry.FileExtension);
                FileHelper.WriteLogs("Scan: " + interfaceEntry.LocalPath + "===> " + lstFile.Count.ToString());
                if (lstFile.Count > 0 && !string.IsNullOrEmpty(interfaceEntry.Prefix))
                {
                    int affectedRows = 0;
                    _dapperContext = new DapperContext(interfaceEntry.Prefix);
                    using IDbConnection conn = _dapperContext.CreateConnDB;
                    conn.Open();
                    foreach (string fileName in lstFile)
                    {
                        try
                        {
                            var transRaw = JsonConvert.DeserializeObject<List<WinMoneyTransDto>>(System.IO.File.ReadAllText(interfaceEntry.LocalPath + fileName));
                            if (transRaw != null && transRaw.Count > 0)
                            {
                                transRaw.ForEach(x=>x.FileName = fileName);
                                var getDataWinMoneyTrans = conn.Query<WinMoneyTransDto>(@"SELECT * FROM WinMoneyTrans (NOLOCK) WHERE CAST(CrtDate AS DATE) >= CAST(getdate() - 31 AS DATE);").ToList();
                                var lstRequestID_In_DB = getDataWinMoneyTrans.Select(x => x.RequestID).ToArray();
                                var lstRequestID_In_File = transRaw.Select(transRaw => transRaw.RequestID).ToArray();
                                var lstRequestID_Insert = lstRequestID_In_File.Except(lstRequestID_In_DB);
                                if (lstRequestID_Insert.Count() > 0)
                                {
                                    FileHelper.WriteLogs(JsonConvert.SerializeObject(lstRequestID_Insert));
                                    var dataInsert = transRaw.Where(x => lstRequestID_Insert.Contains(x.RequestID)).ToList();
                                    if (dataInsert.Count() > 0)
                                    {
                                        string queryIns = @"INSERT INTO [dbo].[WinMoneyTrans]
                                                            ([RequestID],[RequestTime],[ServiceCode],[ServiceName],[Amount],[Fee],[Total],[Method],[MerchantCode],[StoreID],[CashierID]
                                                            ,[CustomerName],[PhoneCustomer],[BankNumber],[UpdateFlg],[FileName],[CrtDate])
                                                            VALUES (@RequestID, @RequestTime,@ServiceCode,@ServiceName,@Amount,@Fee,@Total,@Method,@MerchantCode,@StoreID,@CashierID
                                                            ,@CustomerName,@PhoneCustomer,@BankNumber, 'N', @FileName, getdate())";
                                        conn.Execute(queryIns, dataInsert);
                                        flag = true;
                                    }
                                }
                            }

                            affectedRows++;
                            if (affectedRows == 100)
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            flag = false;
                            FileHelper.WriteLogs("ImportWinMoneyTrans IDbConnection Exception: " + ex.ToString());
                        }

                        if(flag && interfaceEntry.IsMoveFile)
                        {
                            FileHelper.MoveFileToDestination(interfaceEntry.LocalPath + fileName, interfaceEntry.LocalPathArchived);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("ImportWinMoneyTrans Exception: " + ex.ToString());
            }
        }
    }
}
