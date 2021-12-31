using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.AppService;
using PhucLong.Interface.Central.Database;
using System;
using System.Linq;
using VCM.Common.Helpers;

namespace VCM.PhucLong.Interface
{
    class Program
    {
        static void Main()
        {
            IConfiguration _configuration = new ConfigurationBuilder()
                           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                           .Build();
            try
            {
                string app = _configuration["AppSetting:App"].ToString().ToUpper();
                string set = _configuration["AppSetting:Type"].ToString().ToUpper();
                string back_date = _configuration["AppSetting:EntryDate"].ToString().ToUpper();
                string path_local = _configuration["AppSetting:PathProcess"].ToString();
                string path_archived = _configuration["AppSetting:PathArchived"].ToString();
                string path_master_data = _configuration["AppSetting:PathMasterOdoo"].ToString();

                CentralDbContext centralDbContext = new CentralDbContext();
                var interfaceEntry = centralDbContext.InterfaceEntry.Where(x => x.Blocked == false).ToList();

                if (string.IsNullOrEmpty(back_date))
                {
                    back_date = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                }

                switch (app)
                {
                    case "PLG-INBOUND":

                        InterfaceService interfaceService = new InterfaceService(_configuration, interfaceEntry, centralDbContext);
                        interfaceService.RunInbound(set, path_master_data);

                        FileHelper.WriteLogs("*************************************");
                        FileHelper.WriteLogs("*********** START OUTBOUND **********");
                        interfaceService.RunOutbound(set);

                        break;
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("Main Exception: " + ex.Message.ToString());
            }
        }
    }
}
