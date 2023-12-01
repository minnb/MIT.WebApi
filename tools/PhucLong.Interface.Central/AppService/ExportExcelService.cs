using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using PhucLong.Interface.Central.Database;
using PhucLong.Interface.Central.Models.WCM;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using VCM.Common.Helpers;

namespace PhucLong.Interface.Central.AppService
{
    public class ExportExcelService
    {
        private IConfiguration _config;
        private DapperCentral _dapperContext;
        public ExportExcelService
        (
              IConfiguration config
        )
        {
            _config = config;
            _dapperContext = new DapperCentral(_config);
        }

        public bool ExportSalesPhucLongToWCM(string procedure, string rootFolder, string jobType)
        {
            try
            {
                using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
                conn.Open();

                var sQuery = @" EXEC " + procedure;
                IList<ExportSalesModel> dataList = conn.Query<ExportSalesModel>(sQuery, commandTimeout: 3600).ToList();
                FileHelper.WriteLogs("===> " + sQuery);
                if(dataList.Count <= 0)
                {
                    return true;
                }

                string fileName = jobType + "_" + dataList.FirstOrDefault().OrderDate.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("yyyyMMddHHmmssf") + ".xlsx";
                FileInfo file = new FileInfo(Path.Combine(rootFolder, fileName));
                if (File.Exists(rootFolder + fileName))
                {
                    File.Delete(rootFolder + fileName);
                }

                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    
                    
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(DateTime.Now.ToString("dd-MM-yyyy"));
                    worksheet.DefaultColWidth = 9;

                    int totalRows = dataList.Count;
                    for (var c = 1; c <= 9; c++)
                    {
                        worksheet.Column(c).AutoFit();
                        worksheet.Cells[1, c].Style.Font.Bold = true;
                        worksheet.Cells[1, c].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[1, c].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                    }

                    worksheet.Cells[1, 1].Value = "OrderDate";
                    worksheet.Cells[1, 2].Value = "StoreNo";
                    worksheet.Cells[1, 3].Value = "OrderNo";
                    worksheet.Cells[1, 4].Value = "ItemNo";
                    worksheet.Cells[1, 5].Value = "ItemName";
                    worksheet.Cells[1, 6].Value = "Quantity";
                    worksheet.Cells[1, 7].Value = "UnitPrice";
                    worksheet.Cells[1, 8].Value = "DiscountAmount";
                    worksheet.Cells[1, 9].Value = "LineAmount";

                    int i = 0;
                    for (int row = 2; row <= totalRows + 1; row++)
                    {
                        worksheet.Cells[row, 1].Value = dataList[i].OrderDate.ToString("yyyy-MM-dd"); ;
                        worksheet.Cells[row, 2].Value = dataList[i].StoreNo;
                        worksheet.Cells[row, 3].Value = dataList[i].OrderNo;
                        worksheet.Cells[row, 4].Value = dataList[i].ItemNo;
                        worksheet.Cells[row, 5].Value = dataList[i].ItemName;
                        worksheet.Cells[row, 6].Value = dataList[i].Quantity;
                        worksheet.Cells[row, 7].Value = dataList[i].UnitPrice;
                        worksheet.Cells[row, 8].Value = dataList[i].DiscountAmount;
                        worksheet.Cells[row, 9].Value = dataList[i].LineAmount;
                        i++;
                    }
                    package.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("ExportExcelService.ExportSalesPhucLongToWCM Exception: " + ex.Message.ToString());
                return false;
            }
        }
    }

}
