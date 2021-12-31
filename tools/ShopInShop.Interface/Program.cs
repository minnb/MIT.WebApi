using Microsoft.Extensions.Configuration;
using ShopInShop.Interface.Services;
using System;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.Const;

namespace ShopInShop.Interface
{
    class Program
    {
        static IConfiguration _configuration;
        static async Task Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                           .Build();
            try
            {
                Init();
                string app = _configuration["AppSetting:App"].ToString().ToUpper();
                string type = _configuration["AppSetting:Type"].ToString().ToUpper();
                string pathLocal = _configuration["AppSetting:PathProcess"].ToString();
                string pathArchived = _configuration["AppSetting:PathArchived"].ToString();
                FileHelper.WriteLogs("===> START " + app);
                switch (app)
                {
                    case "TEST":
                        //LoyaltyService loyaltyService = new LoyaltyService(_configuration);
                        //await loyaltyService.GetDataRawLoyaltyAsync(pathLocal, pathArchived);

                        HifreshTransService hifresh1 = new HifreshTransService(_configuration);
                        await hifresh1.TransfferDataToHF(app, type, pathLocal, pathArchived);
                        break;

                    case "HIFRESH":
                        HifreshTransService hifresh = new HifreshTransService(_configuration);
                        await hifresh.TransfferDataToHF(app, type, pathLocal, pathArchived);

                        break;

                    case "LOYALTY-CROWNX":
                        LoyaltyService loyaltyService1 = new LoyaltyService(_configuration);
                        await loyaltyService1.GetDataRawLoyaltyAsync(pathLocal, pathArchived);
                        break;
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("Main Exception: " + ex.Message.ToString());
            }
        }

        static void Init()
        {
            SftpConst.SftpHostOS = _configuration["SftpConfig:HostOS"].ToString();
            SftpConst.SftpHost = _configuration["SftpConfig:Host"].ToString();
            SftpConst.SftpPort = int.Parse(_configuration["SftpConfig:Port"].ToString());

            SftpConst.SftpUserName = _configuration["SftpConfig:Username"].ToString();
            SftpConst.SftpPassword = _configuration["SftpConfig:Password"].ToString();

            SftpConst.Sftp_local_process = _configuration["SftpConfig:Path_local_process"].ToString();
            SftpConst.Sftp_local_archive = _configuration["SftpConfig:Path_local_archive"].ToString();

            SftpConst.Sftp_sftp_process = _configuration["SftpConfig:Path_sftp_process"].ToString();
            SftpConst.Sftp_sftp_archive = _configuration["SftpConfig:Path_sftp_archive"].ToString();

            SftpConst.Sftp_file_extension = _configuration["SftpConfig:File_extension"].ToString();
            SftpConst.Sftp_file_permissions = int.Parse(_configuration["SftpConfig:File_permissions"].ToString());
        }
    }
}
