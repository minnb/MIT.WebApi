using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VCM.Common.Helpers
{
    public static class DirectoryHelper
    {
        public static bool CreateFolder(string _folder)
        {
            try
            {
                if (!Directory.Exists(_folder))
                    Directory.CreateDirectory(_folder);
                return true;
            }
            catch (IOException ex)
            {
                FileHelper.WriteLogs("Create Folder: " + ex.Message.ToString());
                return false;
            }
        }
        public static List<string> GetFileByDir(string path, string filter, int number)
        {
            List<string> lsFile = new List<string>();
            if (path != null)
            {
                string[] files = Directory.GetFiles(path, filter);
                if (files.Length > 0)
                {
                    Array.Sort(files);
                    for (int i = 0; i < files.Length; i++)
                    {
                        lsFile.Add(Path.GetFileName(files[i]));
                        if (number != 0 && i + 1 == number)
                        {
                            break;
                        }
                    }
                }
                return lsFile;
            }
            else
            {
                return lsFile;
            }
        }
        public static List<string> GetFileFromDirOrderByDes(string path, string extention)
        {
            var fileName = (new DirectoryInfo(path))
                            .GetFiles(extention, SearchOption.AllDirectories).OrderByDescending(x => x.Name)
                            .Select(a => a.Name)
                            .ToList();
            return fileName;
        }
    }
}
