using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.AppService;
using PhucLong.Interface.Central.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;

namespace VCM.PhucLong.Interface
{
    class Program
    {
        [Obsolete]
        static async Task Main(string[] args)
        {
            IConfiguration _configuration = new ConfigurationBuilder()
                           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                           .Build();
            try
            {
                string app = string.Empty;
                string set = string.Empty;
                if (_configuration["AppSetting:Env"].ToString().ToUpper() == "DEV")
                {
                    app = _configuration["AppSetting:App"].ToString().ToUpper();
                    set = _configuration["AppSetting:Type"].ToString().ToUpper();
                }
                else if(_configuration["AppSetting:Env"].ToString().ToUpper() == "PRD" || _configuration["AppSetting:Env"].ToString().ToUpper() == "QAS")
                {
                    app = args[0].ToString();
                    set = args[1].ToString();
                }
                string back_date = _configuration["AppSetting:EntryDate"].ToString().ToUpper();
                string path_local = _configuration["AppSetting:PathProcess"].ToString();
                string path_archived = _configuration["AppSetting:PathArchived"].ToString();
                string path_master_data = _configuration["AppSetting:PathMasterOdoo"].ToString();
                string url_logging = _configuration["AppSetting:Logging"].ToString();

                CentralDbContext centralDbContext = new CentralDbContext();
                var interfaceEntry = centralDbContext.InterfaceEntry.Where(x => x.Blocked == false).ToList();

                if (string.IsNullOrEmpty(back_date))
                {
                    back_date = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                }
                InterfaceService interfaceService = new InterfaceService(_configuration, interfaceEntry, centralDbContext);
                FileHelper.WriteLogs("*******************************************************");
                FileHelper.WriteLogs("============= START "+ app + "_" + set +" =============");
                switch (app)
                {
                    case "PLG-MASTER":
                        interfaceService.RunMasterData();
                        break;
                    case "PLG-INBOUND":
                        await interfaceService.RunInboundAsync(set, path_master_data, url_logging);                       
                        break;
                    case "PLG-OUTBOUND":                       
                        interfaceService.RunOutbound(set, url_logging);
                        break;
                    case "SFTP-UPLOAD":
                        interfaceService.UploadToSFTP(set, url_logging);
                        break;
                }
                FileHelper.WriteLogs("============= FINISHED " + app + " =============");
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("Main Exception: " + ex.Message.ToString());
            }
        }
    }
}
