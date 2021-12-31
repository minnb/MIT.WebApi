using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Const
{
    public static class SftpConst
    {
        public static string SftpHostOS = @"WINDOWS";
        public static string SftpHost = @"10.235.76.65";
        public static int SftpPort = 22;
        public static string SftpUserName = @"abc";
        public static string SftpPassword = @"password";
        public static string Sftp_local_process = @"D:\\ROOT\\Processed\\Inbound\\";
        public static string Sftp_local_archive = @"D:\\ROOT\\Processed\\Inbound\\";
        public static string Sftp_sftp_process = @"D:\\ROOT\\Processed\\Inbound\\";
        public static string Sftp_sftp_archive = @"D:\\ROOT\\Processed\\Inbound\\";
        public static int Sftp_file_permissions = 774;
        public static string Sftp_file_extension = "*.xml";
    }
}
