using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;
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
            ConnectGCP();
            Console.ReadLine();

            //IConfiguration _configuration = new ConfigurationBuilder()
            //               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //               .Build();
            //try
            //{
            //    string app = string.Empty;
            //    string set = string.Empty;
            //    if (_configuration["AppSetting:Env"].ToString().ToUpper() == "DEV")
            //    {
            //        app = _configuration["AppSetting:App"].ToString().ToUpper();
            //        set = _configuration["AppSetting:Type"].ToString().ToUpper();
            //    }
            //    else if (_configuration["AppSetting:Env"].ToString().ToUpper() == "PRD" || _configuration["AppSetting:Env"].ToString().ToUpper() == "QAS")
            //    {
            //        app = args[0].ToString();
            //        set = set = args[1].ToString();
            //    }

            //    DatabaseContext centralDbContext = new DatabaseContext();              
            //    FileHelper.WriteLogs("*******************************************************");
            //    FileHelper.WriteLogs("============= START " + app + "_" + set + " =============");
            //    switch (app)
            //    {
            //        case "PRICE-ENGINE":
            //            MasterDataService masterDataService = new MasterDataService(centralDbContext, _configuration);
            //            masterDataService.GetKeyRedis();
            //            break;
            //    }
            //    FileHelper.WriteLogs("============= FINISHED " + app + " =============");
            //}
            //catch (Exception ex)
            //{
            //    FileHelper.WriteLogs("Main Exception: " + ex.Message.ToString());
            //}
        }
        private static void ConnectGCP()
        {
            string serviceAccountKeyPath = "keys/minhnb-0405b9cb78d8.json";
            var credential = GoogleCredential.FromFile(serviceAccountKeyPath).CreateScoped();

            // Tạo đối tượng BigQueryClient từ Credential
            var client = BigQueryClient.Create("minhnb", credential);

            // Tên dataset và bảng trong BigQuery
            string datasetId = "bigquery-public-data.google_trends";
            string tableId = "international_top_rising_terms";

            // Viết truy vấn SQL
            string query = $"SELECT * FROM `bigquery-public-data.google_trends.international_top_rising_terms` WHERE country_name = @country_name LIMIT 50";

            BigQueryParameter[] parameters = new[]
                {
                    new BigQueryParameter("country_name", BigQueryDbType.String, "Vietnam")
                };

            // Thực hiện truy vấn
            var results = client.ExecuteQuery(query, parameters);

            // In kết quả
            foreach (var row in results)
            {
                Console.WriteLine($"{row["country_name"]}: {row["refresh_date"]} : {row["week"]}: {row["score"]}");
            }
        }
    }
}
