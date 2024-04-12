using Dapper;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Interface.Const;
using Tools.Interface.Helpers;
using Tools.Interface.Services.GCP;
using VCM.Common.Database;
using VCM.Common.Helpers;
using VCM.Shared.Dtos.WinMoney;
using VCM.Shared.Entity.Central;
using VCM.Shared.Entity.Partner;

namespace Tools.Interface.Services
{
    public class GCP_MD_Service
    {
        private DapperContext _dapperContext;
        private int _chunSize = 2100;
        public GCP_MD_Service()
        {

        }
        private DataTable ReadCSV(string csvFilePath, string delimiters)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using TextFieldParser parser = new TextFieldParser(csvFilePath, Encoding.UTF8);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(delimiters); // Các trường được phân tách bằng dấu phẩy
                                                  // Đọc hàng đầu tiên làm tiêu đề cột
                string[] headers = parser.ReadFields();
                // Tạo DataTable để lưu trữ dữ liệu từ tệp CSV
                foreach (string header in headers)
                {
                    dataTable.Columns.Add(header); // Thêm cột cho mỗi tiêu đề
                }
                // Đọc dữ liệu từ các hàng còn lại và thêm vào DataTable
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    dataTable.Rows.Add(fields);
                }
                return dataTable;
            }
            catch
            {
                return null;
            }
        }
        public void GCP_MCH_INFO(InterfaceEntry interfaceEntry)
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
                        if (fileName.Contains(interfaceEntry.StoreProName))
                        {
                            FileHelper.WriteLogs("fileName: " + fileName);
                            try
                            {
                                var dataCSV = ReadCSV(interfaceEntry.LocalPath + fileName, ",");
                                if (dataCSV != null && dataCSV.Rows.Count > 0)
                                {
                                    var getDataDB = conn.Query<GCP_MCH_INFO_DATA>(@"SELECT [FIELD_ID],[MCH1_ID],[MCH2_ID],[MCH3_ID],[MCH4_ID],[MCH5_ID],[MCH6_ID],[MCH1_NAME],[MCH2_NAME]
                                                                ,[MCH3_NAME],[MCH4_NAME],[MCH5_NAME],[MCH6_NAME] FROM [dbo].[GCP_MCH] (NOLOCK) ORDER BY [FIELD_ID];").ToList();
                                    
                                    var lstID_DB = getDataDB.Select(x => x.FIELD_ID).ToArray();
                                    var lstID_File = DataTableHelper.GetColumnValues<string>(dataCSV, "FIELD_ID").ToArray();
                                    var lstID_Insert = lstID_File.Except(lstID_DB).ToArray();
                                    FileHelper.WriteLogs("lstID_Insert: " + lstID_Insert.Count().ToString());
                                    if (lstID_Insert.Length > 0)
                                    {
                                        var dataTableResult = DataTableHelper.GetDataWithKeys(dataCSV, "FIELD_ID", lstID_Insert);
                                        var chunks = DataTableHelper.ChunkSizeDataTable(dataTableResult, _chunSize);
                                        Console.WriteLine("total rows: " + chunks.Count);
                                        foreach (var chunk in chunks)
                                        {
                                            var dataIns = Mapping_GCP_MCH(chunk);
                                            conn.Execute(@"DELETE [GCP_MCH] WHERE [FIELD_ID] IN (@FIELD_ID);", dataIns);

                                            string queryINS = @"INSERT INTO [dbo].[GCP_MCH]
                                                                ([MCH1_ID],[MCH2_ID],[MCH3_ID],[MCH4_ID],[MCH5_ID],[MCH6_ID],[MCH1_NAME],[MCH2_NAME]
                                                                ,[MCH3_NAME],[MCH4_NAME],[MCH5_NAME],[MCH6_NAME],[INSERT_DATE],[FIELD_ID])
                                                             VALUES (@MCH1_ID,@MCH2_ID,@MCH3_ID,@MCH4_ID,@MCH5_ID,@MCH6_ID,@MCH1_NAME,@MCH2_NAME
                                                                 ,@MCH3_NAME,@MCH4_NAME,@MCH5_NAME,@MCH6_NAME,@INSERT_DATE,@FIELD_ID)";
                                            conn.Execute(queryINS, dataIns);
                                            flag = true;
                                            Console.WriteLine("OK");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Not found new data");
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
                                FileHelper.WriteLogs("GCP_MCH_INFO IDbConnection Exception: " + ex.ToString());
                            }
                            if (flag && interfaceEntry.IsMoveFile)
                            {
                                FileHelper.MoveFileToDestination(interfaceEntry.LocalPath + fileName, interfaceEntry.LocalPathArchived);
                                FileHelper.WriteLogs("moved: " + fileName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("GCP_MCH_INFO Exception: " + ex.ToString());
            }
        }
        public void GCP_PRODUCT_UNIT(InterfaceEntry interfaceEntry)
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
                    foreach (string fileName in lstFile)
                    {
                        if (fileName.Contains(interfaceEntry.StoreProName))
                        {
                            FileHelper.WriteLogs("fileName: " + fileName);
                            try
                            {
                                var dataCSV = ReadCSV(interfaceEntry.LocalPath + fileName, ",");
                                if (dataCSV != null && dataCSV.Rows.Count > 0)
                                {
                                    using IDbConnection conn = _dapperContext.CreateConnDB;
                                    conn.Open();
                                    var getDataDB = conn.Query<GCP_PRODUCT>(@"SELECT * FROM [dbo].[GCP_PRODUCT_UNIT] (NOLOCK);").ToList();
                                    var lstID_DB = getDataDB.Select(x => x.PRODUCT_ID).ToArray();
                                    var lstID_File = DataTableHelper.GetColumnValues<string>(dataCSV, "FIELD_ID").ToArray();
                                    var lstID_delete = lstID_File.Except(lstID_DB).ToArray();
                                    FileHelper.WriteLogs("lstID_delete: " + lstID_delete.Count().ToString());
                                    if(lstID_delete.Length > 0)
                                    {
                                        var dataTableResult = DataTableHelper.GetDataWithKeys(dataCSV, "FIELD_ID", lstID_delete);
                                        var chunks_Delete = DataTableHelper.ChunkSizeDataTable(dataTableResult, _chunSize);
                                        Console.WriteLine("total rows delete: " + chunks_Delete.Count);
                                        foreach (var chunk in chunks_Delete)
                                        {
                                            var dataIns = Mapping_GCP_PRODUCT_UNIT(chunk);
                                            conn.Execute(@"DELETE [GCP_PRODUCT_UNIT] WHERE [FIELD_ID] IN (@FIELD_ID);", dataIns);
                                            Console.WriteLine("DELETE " + dataIns.Count);
                                        }
                                    }

                                    //INSERT
                                    var chunks = DataTableHelper.ChunkSizeDataTable(dataCSV, _chunSize);                                   
                                    var option = new ParallelOptions { MaxDegreeOfParallelism = 20 };
                                    Console.WriteLine("total rows insert: " + chunks.Count);
                                    FileHelper.WriteLogs("total rows insert: " + chunks.Count.ToString());

                                    Parallel.ForEach(chunks, chunk =>
                                    {
                                        var dataIns = Mapping_GCP_PRODUCT_UNIT(chunk);
                                        using IDbConnection conn = _dapperContext.CreateConnDB;
                                        conn.Open();
                                        conn.Execute(GCP_CSV_COLUMN.INSERT_GCP_PRODUCT_UNIT(), dataIns, commandTimeout:720);

                                        Console.WriteLine("inserted " + dataIns.Count);
                                        FileHelper.WriteLogs("inserted: " + dataIns.Count.ToString());
                                    });

                                    flag = true;
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
                                FileHelper.WriteLogs("GCP_PRODUCT_UNIT IDbConnection Exception: " + ex.ToString());
                            }
                            if (flag && interfaceEntry.IsMoveFile)
                            {
                                FileHelper.MoveFileToDestination(interfaceEntry.LocalPath + fileName, interfaceEntry.LocalPathArchived);
                                FileHelper.WriteLogs("moved: " + fileName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("GCP_PRODUCT_UNIT Exception: " + ex.ToString());
            }
        }
        public void GCP_PRODUCT(InterfaceEntry interfaceEntry)
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
                        if (fileName.Contains(interfaceEntry.StoreProName))
                        {
                            FileHelper.WriteLogs("fileName: " + fileName);
                            try
                            {
                                var dataCSV = ReadCSV(interfaceEntry.LocalPath + fileName, ",");
                                if (dataCSV != null && dataCSV.Rows.Count > 0)
                                {
                                    var getDataDB = conn.Query<GCP_PRODUCT>(@"SELECT * FROM [dbo].[GCP_PRODUCT] (NOLOCK);").ToList();
                                    var lstID_DB = getDataDB.Select(x => x.PRODUCT_ID).ToArray();
                                    var lstID_File = DataTableHelper.GetColumnValues<string>(dataCSV, "PRODUCT_ID").ToArray();
                                    var lstID_Insert = lstID_File.Except(lstID_DB).ToArray();
                                    FileHelper.WriteLogs("lstID_Insert: " + lstID_Insert.Count().ToString());
                                    
                                    if (lstID_Insert.Length > 0)
                                    {
                                        var dataTableResult = DataTableHelper.GetDataWithKeys(dataCSV, "PRODUCT_ID", lstID_Insert);
                                        var chunks = DataTableHelper.ChunkSizeDataTable(dataTableResult, _chunSize);
                                        Console.WriteLine("total rows: " + chunks.Count);
                                        foreach (var chunk in chunks)
                                        {
                                            var dataIns = Mapping_GCP_PRODUCT(chunk);
                                            conn.Execute(@"DELETE [GCP_PRODUCT] WHERE [PRODUCT_ID] IN (@PRODUCT_ID);", dataIns);

                                            conn.Execute(GCP_CSV_COLUMN.INSERT_GCP_PRODUCT(), dataIns);
                                            flag = true;
                                            Console.WriteLine("inserted " + dataIns.Count);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Not found data");
                                        //var dtDB = DataTableHelper.ConvertListToDataTable(getDataDB);
                                        //var dtCSV = DataTableHelper.ReorderColumns(dataCSV, GCP_CSV_COLUMN.PRODUCT_DT());
                                        //var dataChange = DataTableHelper.GetDifferentRows(dataCSV, dtDB);
                                        //if (dataChange.Rows.Count > 0)
                                        //{
                                        //    Console.WriteLine("Found change data " + dataChange.Rows.Count);
                                        //    var chunks = DataTableHelper.ChunkSizeDataTable(dataChange, 999);
                                        //    Console.WriteLine("total rows: " + chunks.Count);
                                        //    foreach (var chunk in chunks)
                                        //    {
                                        //        var dataIns = Mapping_GCP_PRODUCT(chunk);
                                        //        //DELETE
                                        //        conn.Execute(@"DELETE [GCP_PRODUCT_UNIT] WHERE [PRODUCT_ID] IN (@PRODUCT_ID);", dataIns);
                                        //        //INSERT
                                        //        conn.Execute(GCP_CSV_COLUMN.INSERT_GCP_PRODUCT(), dataIns);
                                        //        flag = true;
                                        //        Console.WriteLine("OK");
                                        //    }
                                        //    FileHelper.WriteLogs("Inserted change data: " + dataChange.Rows.Count.ToString());
                                        //}
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
                                FileHelper.WriteLogs("GCP_PRODUCT IDbConnection Exception: " + ex.ToString());
                            }
                            if (flag && interfaceEntry.IsMoveFile)
                            {
                                FileHelper.MoveFileToDestination(interfaceEntry.LocalPath + fileName, interfaceEntry.LocalPathArchived);
                                FileHelper.WriteLogs("moved: " + fileName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("GCP_PRODUCT Exception: " + ex.ToString());
            }
        }
        public void GCP_STORE(InterfaceEntry interfaceEntry)
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
                        FileHelper.WriteLogs("fileName: " + fileName);
                        if (fileName.Contains(interfaceEntry.StoreProName))
                        {
                            try
                            {
                                var dataCSV = ReadCSV(interfaceEntry.LocalPath + fileName, ",");
                                if (dataCSV != null && dataCSV.Rows.Count > 0)
                                {
                                    var chunks = DataTableHelper.ChunkSizeDataTable(dataCSV, _chunSize);
                                    Console.WriteLine("total rows: " + chunks.Count);
                                    foreach (var chunk in chunks)
                                    {
                                        var dataIns = Mapping_GCP_STORE(chunk);
                                        conn.Execute(@"DELETE [GCP_STORE] WHERE [STORE_ID] IN (@STORE_ID);", dataIns);

                                        conn.Execute(GCP_CSV_COLUMN.INSERT_GCP_STORE(), dataIns);
                                        flag = true;
                                        Console.WriteLine("inserted " + dataIns.Count);
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
                                FileHelper.WriteLogs("GCP_STORE IDbConnection Exception: " + ex.ToString());
                            }
                            if (flag && interfaceEntry.IsMoveFile)
                            {
                                FileHelper.WriteLogs("moved: " + fileName);
                                FileHelper.MoveFileToDestination(interfaceEntry.LocalPath + fileName, interfaceEntry.LocalPathArchived);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("GCP_STORE Exception: " + ex.ToString());
            }
        }
        public void GCP_SALE_BY_BU_REGION(InterfaceEntry interfaceEntry)
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
                        if (fileName.Contains(interfaceEntry.StoreProName))
                        {
                            FileHelper.WriteLogs("fileName: " + fileName);
                            try
                            {
                                var dataCSV = ReadCSV(interfaceEntry.LocalPath + fileName, ",");
                                if (dataCSV != null && dataCSV.Rows.Count > 0)
                                {
                                    var getDataDB = conn.Query<GCP_SALE_BY_BU_REGION>(@"SELECT * FROM [dbo].[GCP_SALE_BY_BU_REGION] (NOLOCK);").ToList();
                                    var lstID_DB = getDataDB.Select(x => x.FIELD_ID).ToArray();
                                    var lstID_File = DataTableHelper.GetColumnValues<string>(dataCSV, "FIELD_ID").ToArray();
                                    var lstID_Insert = lstID_File.Except(lstID_DB).ToArray();
                                    FileHelper.WriteLogs("lstID_Insert: " + lstID_Insert.Count().ToString());
                                    if (lstID_Insert.Length > 0)
                                    {
                                        var dataTableResult = DataTableHelper.GetDataWithKeys(dataCSV, "FIELD_ID", lstID_Insert);
                                        var chunks = DataTableHelper.ChunkSizeDataTable(dataTableResult, _chunSize);
                                        Console.WriteLine("total rows: " + chunks.Count);
                                        foreach (var chunk in chunks)
                                        {
                                            var dataIns = Mapping_GCP_SALE_BY_BU_REGION(chunk);
                                            var queryDelete = @"DELETE [GCP_SALE_BY_BU_REGION] WHERE [FIELD_ID] IN (@FIELD_ID);";

                                            conn.Execute(queryDelete + GCP_CSV_COLUMN.INSERT_GCP_SALE_BY_BU_REGION(), dataIns);
                                            flag = true;
                                            Console.WriteLine("OK");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Not found data");
                                        //var dtDB = DataTableHelper.ConvertListToDataTable(getDataDB);
                                        //var dtCSV = DataTableHelper.ReorderColumns(dataCSV, GCP_CSV_COLUMN.PRODUCT_DT());
                                        //var dataChange = DataTableHelper.GetDifferentRows(dataCSV, dtDB);
                                        //if (dataChange.Rows.Count > 0)
                                        //{
                                        //    Console.WriteLine("Found change data " + dataChange.Rows.Count);
                                        //    var chunks = DataTableHelper.ChunkSizeDataTable(dataChange, 999);
                                        //    Console.WriteLine("total rows: " + chunks.Count);
                                        //    foreach (var chunk in chunks)
                                        //    {
                                        //        var dataIns = Mapping_GCP_SALE_BY_BU_REGION(chunk);
                                        //        //DELETE
                                        //        conn.Execute(@"DELETE [GCP_SALE_BY_BU_REGION] WHERE [FIELD_ID] IN (@FIELD_ID);", dataIns);
                                        //        //INSERT
                                        //        conn.Execute(GCP_CSV_COLUMN.INSERT_GCP_SALE_BY_BU_REGION(), dataIns);
                                        //        flag = true;
                                        //        Console.WriteLine("OK");
                                        //    }
                                        //    FileHelper.WriteLogs("Inserted change data: " + dataChange.Rows.Count.ToString());
                                        //}
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
                                FileHelper.WriteLogs("GCP_SALE_BY_BU_REGION IDbConnection Exception: " + ex.ToString());
                            }
                            if (flag && interfaceEntry.IsMoveFile)
                            {
                                FileHelper.MoveFileToDestination(interfaceEntry.LocalPath + fileName, interfaceEntry.LocalPathArchived);
                                FileHelper.WriteLogs("moved: " + fileName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("GCP_SALE_BY_BU_REGION Exception: " + ex.ToString());
            }
        }
        private List<GCP_MCH_INFO> Mapping_GCP_MCH(List<DataRow> dataRows)
        {
            List<GCP_MCH_INFO> lstData = new List<GCP_MCH_INFO>();
            foreach (var dataRow in dataRows)
            {
                lstData.Add(new GCP_MCH_INFO()
                {
                    MCH1_ID = dataRow["MCH1_ID"].ToString(),
                    MCH2_ID = dataRow["MCH2_ID"].ToString(),
                    MCH3_ID = dataRow["MCH3_ID"].ToString(),
                    MCH4_ID = dataRow["MCH4_ID"].ToString(),
                    MCH5_ID = dataRow["MCH5_ID"].ToString(),
                    MCH6_ID = dataRow["MCH6_ID"].ToString(),
                    MCH1_NAME = dataRow["MCH1_NAME"].ToString(),
                    MCH2_NAME = dataRow["MCH2_NAME"].ToString(),
                    MCH3_NAME = dataRow["MCH3_NAME"].ToString(),
                    MCH4_NAME = dataRow["MCH4_NAME"].ToString(),
                    MCH5_NAME = dataRow["MCH5_NAME"].ToString(),
                    MCH6_NAME = dataRow["MCH6_NAME"].ToString(),
                    FIELD_ID = dataRow["FIELD_ID"].ToString(),
                    INSERT_DATE = DateTime.Now
                });
            }
            return lstData;
        }
        private List<GCP_PRODUCT> Mapping_GCP_PRODUCT(List<DataRow> dataRows)
        {
            List<GCP_PRODUCT> lstData = new List<GCP_PRODUCT>();
            foreach (var dataRow in dataRows)
            {
                lstData.Add(new GCP_PRODUCT()
                {
                    PRODUCT_ID = dataRow["PRODUCT_ID"].ToString(),
                    PRODUCT_NAME = dataRow["PRODUCT_NAME"].ToString(),
                    BASE_UOM = dataRow["BASE_UOM"].ToString(),
                    PRODUCT_TYPE = dataRow["PRODUCT_TYPE"].ToString(),
                    MCH1_ID = dataRow["MCH1_ID"].ToString(),
                    MCH2_ID = dataRow["MCH2_ID"].ToString(),
                    MCH3_ID = dataRow["MCH3_ID"].ToString(),
                    MCH4_ID = dataRow["MCH4_ID"].ToString(),
                    MCH5_ID = dataRow["MCH5_ID"].ToString(),
                    MCH6_ID = dataRow["MCH6_ID"].ToString(),
                    MCH1_NAME = dataRow["MCH1_NAME"].ToString(),
                    MCH2_NAME = dataRow["MCH2_NAME"].ToString(),
                    MCH3_NAME = dataRow["MCH3_NAME"].ToString(),
                    MCH4_NAME = dataRow["MCH4_NAME"].ToString(),
                    MCH5_NAME = dataRow["MCH5_NAME"].ToString(),
                    MCH6_NAME = dataRow["MCH6_NAME"].ToString(),
                    TOTAL_SHELF_LIFE = dataRow["TOTAL_SHELF_LIFE"].ToString(),
                    REMAINING_SHELF_LIFE = dataRow["REMAINING_SHELF_LIFE"].ToString(),
                    MANUFACTURER_ID = dataRow["MANUFACTURER_ID"].ToString(),
                    MANUFACTURER_NAME = dataRow["MANUFACTURER_NAME"].ToString(),
                    MANUFACTURER = dataRow["MANUFACTURER"].ToString(),
                    SUB_MANUFACTURER_ID = dataRow["SUB_MANUFACTURER_ID"].ToString(),
                    SUB_MANUFACTURER_NAME = dataRow["SUB_MANUFACTURER_NAME"].ToString(),
                    BRAND_ID = dataRow["BRAND_ID"].ToString(),
                    BRAND = dataRow["BRAND"].ToString(),
                    BRAND_NAME = dataRow["BRAND_NAME"].ToString(),
                    SUB_BRAND_ID = dataRow["SUB_BRAND_ID"].ToString(),
                    SUB_BRAND_NAME = dataRow["SUB_BRAND_NAME"].ToString(),
                    NAME = dataRow["NAME"].ToString(),
                    TEMPERATURE = dataRow["TEMPERATURE"].ToString(),
                    ORIGIN = dataRow["ORIGIN"].ToString(),
                    COUNTRY_OF_ORIGIN = dataRow["COUNTRY_OF_ORIGIN"].ToString(),
                    ATTRIBUTE12 = dataRow["ATTRIBUTE12"].ToString(),
                    MODEL_DESC = dataRow["MODEL_DESC"].ToString(),
                    VAT_CODE = dataRow["VAT_CODE"].ToString(),
                    VAT_DESC = dataRow["VAT_DESC"].ToString(),
                    INSERT_DATE = DateTime.Now
                });
            }
            return lstData;
        }
        private List<GCP_PRODUCT_UNIT> Mapping_GCP_PRODUCT_UNIT(List<DataRow> dataRows)
        {
            List<GCP_PRODUCT_UNIT> lstData = new List<GCP_PRODUCT_UNIT>();
            foreach (var dataRow in dataRows)
            {
                lstData.Add(new GCP_PRODUCT_UNIT()
                {
                    PRODUCT_ID = dataRow["PRODUCT_ID"].ToString(),
                    UNIT = dataRow["UNIT"].ToString(),
                    BARCODE = dataRow["BARCODE"].ToString(),
                    BARCODE_FULL = dataRow["BARCODE_FULL"].ToString(),
                    NUMERATOR = NumberHelper.StrToDecimal(dataRow["NUMERATOR"].ToString()),
                    BASE_UOM = dataRow["BASE_UOM"].ToString(),
                    INSERT_DATE = DateTime.Now,
                    FIELD_ID = dataRow["FIELD_ID"].ToString(),
                });
            }
            return lstData;
        }
        private List<GCP_STORE> Mapping_GCP_STORE(List<DataRow> dataRows)
        {
            List<GCP_STORE> lstData = new List<GCP_STORE>();
            foreach (var dataRow in dataRows)
            {
                lstData.Add(new GCP_STORE()
                {
                    STORE_ID = dataRow["STORE_ID"].ToString(),
                    STORE_NAME = dataRow["STORE_NAME"].ToString(),
                    STORE_DESCRIPTION = dataRow["STORE_DESCRIPTION"].ToString(),
                    STORE_OPEN_DATE = dataRow["STORE_OPEN_DATE"].ToString(),
                    STORE_CLOSED_DATE = dataRow["STORE_CLOSED_DATE"].ToString(),
                    BUSINESS_UNIT = dataRow["BUSINESS_UNIT"].ToString(),
                    REGION = dataRow["REGION"].ToString(),
                    REGION_VN = dataRow["REGION_VN"].ToString(),
                    CITY_VN = dataRow["CITY_VN"].ToString(),
                    DISTRICT = dataRow["DISTRICT"].ToString(),
                    WARD = dataRow["WARD"].ToString(),
                    ADDRESS = dataRow["ADDRESS"].ToString(),
                    CUA_HANG_TRUONG = dataRow["CUA_HANG_TRUONG"].ToString(),
                    SDT_CHT = dataRow["SDT_CHT"].ToString(),
                    SDT_CH = dataRow["SDT_CH"].ToString(),
                    EMAIL_CH = dataRow["EMAIL_CH"].ToString(),
                    QLKV = dataRow["QLKV"].ToString(),
                    SDT_QLKV = dataRow["SDT_QLKV"].ToString(),
                    EMAIL_QLKV = dataRow["EMAIL_QLKV"].ToString(),
                    GDV = dataRow["GDV"].ToString(),
                    EMAIL_GDV = dataRow["EMAIL_GDV"].ToString(),
                    GDM = dataRow["GDM"].ToString(),
                    EMAIL_GDM = dataRow["EMAIL_GDM"].ToString(),
                    LATITUDE = dataRow["LATITUDE"].ToString(),
                    LONGTITUDE = dataRow["LONGTITUDE"].ToString(),
                    SIZE_V = dataRow["SIZE_V"].ToString(),
                    CONCEPT_ID = dataRow["CONCEPT_ID"].ToString(),
                    CONCEPT = dataRow["CONCEPT"].ToString(),
                    SUPPLY_REGION = dataRow["SUPPLY_REGION"].ToString(),
                    SIZE_ST = dataRow["SIZE_ST"].ToString(),
                    HUB_HANG_KHO = dataRow["HUB_HANG_KHO"].ToString(),
                    STORE_STATUS_DESC = dataRow["STORE_STATUS_DESC"].ToString(),
                    REF_FC_STORE = dataRow["REF_FC_STORE"].ToString(),
                    SUPPLY_STORE = dataRow["SUPPLY_STORE"].ToString(),
                    REF_FC_CODE = dataRow["REF_FC_CODE"].ToString(),
                    CITY = dataRow["CITY"].ToString(),
                    REGION_DOMAIN = dataRow["REGION_DOMAIN"].ToString(),
                    REGION_DOMAIN_VN = dataRow["REGION_DOMAIN_VN"].ToString(),
                    STORE_STATUS_ID = dataRow["STORE_STATUS_ID"].ToString(),
                    IS_WIN_PLUS = dataRow["IS_WIN_PLUS"].ToString(),
                    INSERT_DATE = DateTime.Now
                });
            }
            return lstData;
        }
        private List<GCP_SALE_BY_BU_REGION> Mapping_GCP_SALE_BY_BU_REGION(List<DataRow> dataRows)
        {
            List<GCP_SALE_BY_BU_REGION> lstData = new List<GCP_SALE_BY_BU_REGION>();
            foreach (var dataRow in dataRows)
            {
                lstData.Add(new GCP_SALE_BY_BU_REGION()
                {
                    FIELD_ID = dataRow["FIELD_ID"].ToString(),
                    PRODUCT_ID = dataRow["PRODUCT_ID"].ToString(),
                    BU = dataRow["BU"].ToString(),
                    REGION = dataRow["REGION"].ToString(),
                    AVG_REVENUE = NumberHelper.StrToDouble(dataRow["AVG_REVENUE"].ToString()),
                    AVG_QTY = NumberHelper.StrToDouble(dataRow["AVG_QTY"].ToString()),
                    INSERT_DATE = DateTime.Now
                });
            }
            return lstData;
        }
    }
}
