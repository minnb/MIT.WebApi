using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Odoo.AppService;
using System;
using System.Threading.Tasks;
using VCM.Common.Helpers;

namespace PhucLong.Interface.Odoo
{
    class Program
    {
        static async Task Main()
        {
            try
            {
                IConfiguration _configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .Build();

                string app = _configuration["AppSetting:App"].ToString().ToUpper();
                string type = _configuration["AppSetting:Type"].ToString().ToUpper();
                string back_date = _configuration["AppSetting:EntryDate"].ToString();
                string path_local = _configuration["AppSetting:PathProcess"].ToString();
                string path_archived = _configuration["AppSetting:PathArchived"].ToString();
                string path_master_data = _configuration["AppSetting:PathMasterOdoo"].ToString();

                if (string.IsNullOrEmpty(back_date))
                {
                    back_date = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd");
                }

                FileHelper.WriteLogs("=========================================");
                FileHelper.WriteLogs("****** RUN: " + app + " === date: " + back_date);

                switch (app)
                {
                    case "POS-ODOO":
                        InterfaceEntryService interfaceEntryService = new InterfaceEntryService(_configuration);
                        interfaceEntryService.RunInterfaceEntry(type, back_date);

                        break;
                    case "ODOO-ARCHIVE":
                        ArchiveDataService archiveDataService = new ArchiveDataService(_configuration);
                        FileHelper.WriteLogs("*** START ARCHIVE ***");
                        await archiveDataService.ArchivePosStagingAsync(_configuration["ConnectionStrings:PostgresSQL"].ToString(),back_date);
                        break;
                }

                FileHelper.WriteLogs("****** FINISHED " + app + " ******");
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("Main Exception: " + ex.Message.ToString());
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }
}
