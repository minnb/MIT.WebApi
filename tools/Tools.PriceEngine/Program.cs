using Microsoft.Extensions.Configuration;
using System;
using Tools.PriceEngine.AppServices;
using Tools.PriceEngine.Database;
using VCM.Common.Helpers;

namespace Tools.PriceEngine
{
    class Program
    {
        static void Main(string[] args)
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
                else if (_configuration["AppSetting:Env"].ToString().ToUpper() == "PRD" || _configuration["AppSetting:Env"].ToString().ToUpper() == "QAS")
                {
                    app = args[0].ToString();
                    set = set = args[1].ToString();
                }

                DatabaseContext centralDbContext = new DatabaseContext();              
                FileHelper.WriteLogs("*******************************************************");
                FileHelper.WriteLogs("============= START " + app + "_" + set + " =============");
                switch (app)
                {
                    case "PRICE-ENGINE":
                        MasterDataService masterDataService = new MasterDataService(centralDbContext, _configuration);
                        masterDataService.GetKeyRedis();
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
