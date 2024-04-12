using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;

namespace Tools.Interface.Helpers
{
    public static class DataTableHelper
    {
        public static DataTable ReorderColumns(DataTable table, string[] columnNames)
        {
            // Tạo DataTable mới với thứ tự cột được chỉ định
            DataTable newTable = new DataTable();
            foreach (string columnName in columnNames)
            {
                newTable.Columns.Add(columnName, table.Columns[columnName].DataType);
            }

            // Sao chép dữ liệu từ DataTable ban đầu vào DataTable mới
            foreach (DataRow row in table.Rows)
            {
                DataRow newRow = newTable.Rows.Add();
                foreach (string columnName in columnNames)
                {
                    newRow[columnName] = row[columnName];
                }
            }

            return newTable;
        }
        public static DataTable ConvertListToDataTable<T>(List<T> list)
        {
            DataTable dataTable = new DataTable();
            if (list != null && list.Count > 0)
            {
                // Lấy danh sách các thuộc tính của đối tượng trong danh sách
                PropertyInfo[] properties = typeof(T).GetProperties();

                // Thêm các cột vào DataTable với tên là tên của thuộc tính
                foreach (PropertyInfo prop in properties)
                {
                    dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }

                // Thêm dữ liệu từ danh sách vào DataTable
                foreach (T item in list)
                {
                    DataRow row = dataTable.NewRow();
                    foreach (PropertyInfo prop in properties)
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                    dataTable.Rows.Add(row);
                }
            }
            return dataTable;
        }
        public static DataTable GetDifferentRowsWithParallel(DataTable dataTableA, DataTable dataTableB)
        {
            DataTable differentRows = new DataTable();
            differentRows.Columns.AddRange(dataTableA.Columns.Cast<DataColumn>().Select(c => new DataColumn(c.ColumnName, c.DataType)).ToArray());
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4
            };

            List<DataRow> rowsB = dataTableB.AsEnumerable().ToList();

            Parallel.ForEach(dataTableA.AsEnumerable(), options, rowA =>
            {
                bool isEqual = false;
                foreach (DataRow rowB in rowsB)
                {
                    if (DataRowEquals(rowA, rowB))
                    {
                        isEqual = true;
                        break;
                    }
                }
                if (!isEqual)
                {
                    lock (differentRows)
                    {
                        differentRows.ImportRow(rowA);
                    }
                }
            });
            return differentRows;
        }

        public static DataTable GetDifferentRows(DataTable dataTableA, DataTable dataTableB)
        {
            DataTable differentRows = new DataTable();
            differentRows.Columns.AddRange(dataTableA.Columns.Cast<DataColumn>().Select(c => new DataColumn(c.ColumnName, c.DataType)).ToArray());

            foreach (DataRow rowA in dataTableA.Rows)
            {
                bool isEqual = false;
                foreach (DataRow rowB in dataTableB.Rows)
                {
                    if (DataRowEquals(rowA, rowB))
                    {
                        isEqual = true;
                        break;
                    }
                }
                if (!isEqual)
                {
                    differentRows.ImportRow(rowA);
                }
            }
            return differentRows;
        }

        private static bool DataRowEquals(DataRow rowA, DataRow rowB)
        {
            for(int i = 1; i < rowA.ItemArray.Length; i++ )
            {
                if (!rowA[i].Equals(rowB[i]))
                {
                    return false;
                }
            }
            return true;
        }
        public static DataTable GetDifferentRowsByColumn(DataTable dataTable_FILE, DataTable dataTable_DB, string columnName)
        {
            // Liệt kê các dòng trong DataTable A có giá trị cột columnName không tồn tại trong DataTable B
            var differentRows = from rowA in dataTable_FILE.AsEnumerable()
                                join rowB in dataTable_DB.AsEnumerable()
                                on rowA.Field<string>(columnName) equals rowB.Field<string>(columnName) into gj
                                from subRowB in gj.DefaultIfEmpty()
                                where subRowB == null
                                select rowA;

            // Chuyển kết quả thành DataTable
            DataTable result = differentRows.CopyToDataTable();

            return result;
        }

        public static DataTable GetDataWithKeys(DataTable table, string keyColumnName, string[] keys)
        {
            if(keys == null || keys.Length == 0) { return table; }

            // Tạo một DataTable mới để lưu kết quả
            DataTable result = table.Clone();
            // Lặp qua mỗi khóa trong mảng keys
            foreach (string key in keys)
            {
                // Tìm hàng có khóa tương ứng và thêm vào DataTable kết quả
                DataRow[] rows = table.Select($"{keyColumnName} = '{key}'");
                foreach (DataRow row in rows)
                {
                    result.ImportRow(row);
                }
            }

            return result;
        }
        public static T[] GetColumnValues<T>(DataTable table, string columnName)
        {
            // Kiểm tra nếu DataTable không có cột có tên như columnName
            if (!table.Columns.Contains(columnName))
            {
                throw new ArgumentException($"Column '{columnName}' not found in the DataTable.");
            }

            // Khởi tạo mảng để lưu giá trị của cột
            T[] values = new T[table.Rows.Count];

            // Lặp qua từng hàng trong DataTable và lấy giá trị của cột
            for (int i = 0; i < table.Rows.Count; i++)
            {
                values[i] = (T)table.Rows[i][columnName];
            }

            return values;
        }
        public static List<List<DataRow>> ChunkSizeDataTable(DataTable dataTable, int chunkSize)
        {
            try
            {
                return dataTable.AsEnumerable()
                                            .AsParallel()
                                            .WithDegreeOfParallelism(Environment.ProcessorCount)
                                            .Select((row, index) => new { Row = row, Index = index })
                                            .GroupBy(x => x.Index / chunkSize)
                                            .Select(g => g.Select(x => x.Row).ToList())
                                            .ToList();
            }
            catch(Exception ex) 
            {
                FileHelper.WriteLogs("ChunkSizeDataTable Exception: " + JsonConvert.SerializeObject(ex));
                return null;
            }
        }
    }
}
