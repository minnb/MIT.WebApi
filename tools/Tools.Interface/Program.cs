using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tools.Interface.Database;
using Tools.Interface.Services;
using VCM.Common.Helpers;

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
                string app = string.Empty;
                string jobName = string.Empty;
                if (_configuration["AppSetting:Env"].ToString().ToUpper() == "DEV")
                {
                    app = _configuration["AppSetting:App"].ToString().ToUpper();
                    jobName = _configuration["AppSetting:Type"].ToString().ToUpper();
                }
                else if (_configuration["AppSetting:Env"].ToString().ToUpper() == "PRD" || _configuration["AppSetting:Env"].ToString().ToUpper() == "QAS")
                {
                    app = args[0].ToString();
                    jobName = jobName = args[1].ToString();
                }

                string type = _configuration["AppSetting:Type"].ToString().ToUpper();
                
                FileHelper.WriteLogs("****** START " + app + " ******");

                InterfaceDbContext centralDbContext = new InterfaceDbContext();
                var interfaceEntry = centralDbContext.InterfaceEntry.Where(x => x.Blocked == false).ToList();
                if(interfaceEntry.Count > 0)
                {
                    InterfaceEntryService interfaceEntryService = new InterfaceEntryService(_configuration, interfaceEntry, centralDbContext);
                    await interfaceEntryService.RunInterfaceAsync(app, jobName);
                }

                FileHelper.WriteLogs("****** END " + app + " ******");
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("Main Exception: " + ex.Message.ToString());
            }
        }


    }
}
