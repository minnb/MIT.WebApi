using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VCM.Shared.API;

namespace WebApi.Core.Common.Constants
{
    public static class StringConts
    {
        public static string[] GetBlackList()
        {
            string[] blackList = { "/index.html",
                                    "/profiler/includes.min.js",
                                    "/swagger/v1/swagger.json",
                                    "/profiler/results",
                                    "/",
                                    "/favicon",
                                    "/swagger-ui-standalone-preset.js",
                                    "/swagger-ui.css",
                                    "/swagger-ui-bundle.js.map",
                                    "/swagger-ui-standalone-preset.js.map",
                                    "/swagger-ui-bundle.js",
                                    "/profiler/includes.min.css"};
            return blackList;
        }
        public static string GetMessageStatus(int status)
        {
            Dictionary<int, string> messages = new Dictionary<int, string>
            {
                { 200, "Successfully" },
                { 801, "Wrong code checksum" },
                { 802, "Order already exists" },
                { 803, "Order not found" },
                { 804, "Error creating order" },
                { 805, "Error processing data" },
                { 806, "Status field is wrong" },
                { 807, "Incorrect cancellation quantity" },
                { 808, "The order has been canceled" },
                { 809, "Warehouse does not belong Online" },
                { 888, "Order does not exist or Data duplication" },
                { 889, "Data type error" }
            };

            return messages[status].ToString();
        }
        public static Meta GetMessageRsp(int status)
        {
            try
            {
                return new Meta()
                {
                    Code = status,
                    Message = GetMessageStatus(status)
                };
            }
            catch (Exception ex)
            {
                return new Meta()
                {
                    Code = 900,
                    Message = ex.Message.ToString()
                };
            }

        }
    }
}
