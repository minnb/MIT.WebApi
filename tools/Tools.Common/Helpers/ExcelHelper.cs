using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tools.Common.Helpers
{
    public static class ExcelHelper
    {
        public static string ExcelToCsv(string file, string targetFolder)
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
    }
}
