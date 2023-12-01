using System;
using System.Collections.Generic;
using System.Text;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;

namespace Tools.Common.Helpers
{
    public static class InterfaceHelper
    {
        public static void UploadFileToSftpSetup(InterfaceEntry interfaceEntry)
        {
            string sftpOS = interfaceEntry.SftpOS.ToString();
            string sftpHost = interfaceEntry.SftpHost.ToString();
            int sftpPort = interfaceEntry.SftpPort;
            string sftpUserName = interfaceEntry.SftpUser.ToString();
            string sftpPassword = interfaceEntry.SftpPass.ToString();
            string sftpPath = interfaceEntry.SftpPath.ToString();
            string fileExtention = interfaceEntry.FileExtension.ToString();
            short sftpPermision = (Int16)interfaceEntry.SfptPermissions;
            bool isMoveFile = interfaceEntry.IsMoveFile;
            string pathLocal = interfaceEntry.LocalPath.ToString();
            string pathAchive = interfaceEntry.LocalPathArchived.ToString();

            if (!string.IsNullOrEmpty(sftpHost))
            {
                FileHelper.WriteLogs("===> Upload file to sftp: " + sftpHost + ":" + sftpPort);
                FileHelper.WriteLogs("===> File: " + fileExtention + " from: " + pathLocal);
                FileHelper.WriteLogs("===> Sftp: " + sftpPath);
                SftpHelper sftpHelper = new SftpHelper(sftpHost, sftpPort, sftpUserName, sftpPassword);
                if (sftpOS.ToUpper() == "WINDOW")
                {
                    sftpHelper.UploadSftpWindow(pathLocal, sftpPath, pathAchive, fileExtention, sftpPermision, isMoveFile);
                }
                else
                {
                    if (sftpOS.ToUpper() == "LINUX")
                    {
                        sftpHelper.UploadSftpLinux(pathLocal, sftpPath, pathAchive, fileExtention, sftpPermision, isMoveFile);
                    }
                    else
                    {
                        sftpHelper.UploadSftpLinux2(pathLocal, sftpPath, pathAchive, fileExtention, sftpPermision, isMoveFile);
                    }
                }
            }
        }
        public static void DownloadFileToSftpSetup(InterfaceEntry interfaceEntry)
        {
            string sftpOS = interfaceEntry.SftpOS.ToString();
            string sftpHost = interfaceEntry.SftpHost.ToString();
            int sftpPort = interfaceEntry.SftpPort;
            string sftpUserName = interfaceEntry.SftpUser.ToString();
            string sftpPassword = interfaceEntry.SftpPass.ToString();
            string sftpPath = interfaceEntry.SftpPath.ToString();
            string fileExtention = interfaceEntry.FileExtension.ToString();
            short sftpPermision = (Int16)interfaceEntry.SfptPermissions;
            bool isMoveFile = interfaceEntry.IsMoveFile;
            string pathLocal = interfaceEntry.LocalPath.ToString();
            string pathAchive = interfaceEntry.LocalPathArchived.ToString();

            if (!string.IsNullOrEmpty(sftpHost))
            {
                FileHelper.WriteLogs("===> Download file to sftp: " + sftpHost + ":" + sftpPort);
                FileHelper.WriteLogs("===> File: " + fileExtention + " from: " + pathLocal);
                FileHelper.WriteLogs("===> Sftp: " + sftpPath);
                SftpHelper sftpHelper = new SftpHelper(sftpHost, sftpPort, sftpUserName, sftpPassword);
                if (sftpOS.ToUpper() == "WINDOW")
                {
                    sftpHelper.DownloadAuthen(sftpPath, pathLocal);
                }
                else
                {
                    sftpHelper.DownloadNoAuthen(sftpPath, pathLocal, isMoveFile);
                }
            }
        }
    }

}
