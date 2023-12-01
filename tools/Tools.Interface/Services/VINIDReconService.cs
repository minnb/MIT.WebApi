using Dapper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Interface.Dtos.VinIDDto;
using VCM.Common.Database;
using VCM.Common.Helpers;

namespace Tools.Interface.Services
{
    public class VINIDReconService
    {
        private DapperContext _dapperContext;
        public VINIDReconService
          (

          )
        {
        }
        public static string ConvertToCsv(string file, string targetFolder)
        {
            var _random = new Random();
            string file_name = string.Empty;
            FileInfo finfo = new FileInfo(file);
            ExcelPackage package = new ExcelPackage(finfo);

            // if targetFolder doesn't exist, create it
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var worksheets = package.Workbook.Worksheets;
            int sheetcount = 0;

            foreach (ExcelWorksheet worksheet in worksheets)
            {
                var index = _random.Next(10, 99);
                sheetcount++;
                var maxColumnNumber = worksheet.Dimension.End.Column;
                var currentRow = new List<string>(maxColumnNumber);
                var totalRowCount = worksheet.Dimension.End.Row + 1;
                //var currentRowNum = 1;
                //No need for a memory buffer, writing directly to a file
                //var memory = new MemoryStream();

                file_name = targetFolder + Path.GetFileNameWithoutExtension(file) + "_" + index.ToString() + ".csv";

                using (var writer = new StreamWriter(file_name, false, Encoding.UTF8))
                {
                    //the rest of the code remains the same

                    for (int i = 1; i < totalRowCount; i++)
                    {
                        //i.Dump();
                        // populate line with semi columns separators
                        string line = "";
                        for (int j = 1; j < worksheet.Dimension.End.Column + 1; j++)
                        {
                            if (worksheet.Cells[i, j].Value != null)
                            {
                                string cell = worksheet.Cells[i, j].Value.ToString() + ",";
                                line += cell;
                            }
                        }
                        // write line
                        writer.WriteLine(line);
                    }
                }
                
            }
            return file_name;
        }
        public async Task Exp_ReconciliationTopUp(string connectionString, string pathLocal, string prefix)
        {
            _dapperContext = new DapperContext(connectionString);
            using IDbConnection conn = _dapperContext.CreateConnDB;
            conn.Open();
            try
            {
                string queryData = @"SELECT ROW_NUMBER() OVER(ORDER BY [InvoiceNo]) AS Stt,
                                     [Store],[Terminal],[InvoiceNo],[PhoneNumber],[Amount],[Currency],[TransactionType],[TransactionTime],[Status],[UpdateFlg],[FileName],[CrtDate]
                                     FROM dbo.VINID_TopUpRecon NOLOCK WHERE UpdateFlg ='N';";
                var data = conn.Query<VINID_TopUpRecon>(queryData).ToList();

                if(data.Count > 0)
                {
                    string fileName = data.FirstOrDefault().TransactionTime.ToString("yyyyMMdd") + "_" + prefix + ".xlxs";

                    FileInfo file = new FileInfo(Path.Combine(pathLocal, fileName));
                    if (File.Exists(pathLocal + fileName))
                    {
                        File.Delete(pathLocal + fileName);
                    }

                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    using (ExcelPackage package = new ExcelPackage(file))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(DateTime.Now.ToString("dd-MM-yyyy"));
                        worksheet.DefaultColWidth = 9;
                        


                        int totalRows = data.Count;
                        for (var c = 1; c <= 10; c++)
                        {
                            worksheet.Column(c).AutoFit();
                            worksheet.Cells[1, c].Style.Font.Bold = true;
                            worksheet.Cells[1, c].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[1, c].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                        }

                        worksheet.Cells[1, 1].Value = "STT";
                        worksheet.Cells[1, 2].Value = "Store";
                        worksheet.Cells[1, 3].Value = "Terminal";
                        worksheet.Cells[1, 4].Value = "InvoiceNo";
                        worksheet.Cells[1, 5].Value = "PhoneNumber";
                        worksheet.Cells[1, 6].Value = "Amount";
                        worksheet.Cells[1, 7].Value = "Currency";
                        worksheet.Cells[1, 8].Value = "TransactionType";
                        worksheet.Cells[1, 9].Value = "TransactionTime";
                        worksheet.Cells[1, 10].Value = "Status";

                        ExcelTextFormat format = new ExcelTextFormat
                        {
                            Delimiter = ';',
                            DataTypes = new eDataTypes[] { eDataTypes.Number, eDataTypes.String }
                        };

                        int i = 0;
                        for (int row = 2; row <= totalRows + 1; row++)
                        {
                            worksheet.Cells[row, 1].Value = data[i].Stt;
                            worksheet.Cells[row, 2].Value = data[i].Store;
                            worksheet.Cells[row, 3].Value = data[i].Terminal;
                            worksheet.Cells[row, 4].Value = data[i].InvoiceNo;
                            worksheet.Cells[row, 5].Value = data[i].PhoneNumber;
                            worksheet.Cells[row, 6].Value = data[i].Amount;
                            worksheet.Cells[row, 7].Value = data[i].Currency;
                            worksheet.Cells[row, 8].Value = data[i].TransactionType;
                            worksheet.Cells[row, 9].Value = data[i].TransactionTime.ToString("dd/MM/yyyy HH:mm:ss");
                            worksheet.Cells[row, 10].Value = data[i].Status;
                            i++;
                        }
                        package.Save();
                    }
                    FileHelper.WriteLogs("===> Saved: " + fileName);
                    foreach (var item in data)
                    {
                        var queryUpdate = @"UPDATE VINID_TopUpRecon SET UpdateFlg = 'Y' 
                                            WHERE Store = '" + item.Store + "'" +
                                            " AND Terminal = '" + item.Terminal + "'  " +
                                            " AND InvoiceNo = '" + item.InvoiceNo + "';";
                       await conn.ExecuteAsync(queryUpdate);
                    }

                    FileHelper.WriteLogs("===> convert csv: " + ConvertToCsv(pathLocal + fileName, pathLocal));
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> Exp_ReconciliationTopUp Exception" + ex.Message.ToString());
            }
        }
    }
}
