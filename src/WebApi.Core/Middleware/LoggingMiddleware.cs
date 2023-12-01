using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Core.Common.Constants;
using WebApi.Core.Common.Handler;

namespace WebApi.Core.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var corrId = string.Format("{0}{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);
            var requestInfo = string.Format("{0} {1}", context.Request.Method, context.Request.Path + context.Request.QueryString);
            string endpoint = context.Request.Path;
            Stream originalBody = context.Response.Body;
            try
            {
                context.Request.EnableBuffering();
                string body;
                using (var streamReader = new System.IO.StreamReader(context.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
                    body = await streamReader.ReadToEndAsync();

                context.Request.Body.Position = 0;
                context.Items["body"] = body;

                if (!requestInfo.Contains("swagger"))
                {
                    _logger.LogWarning(string.Format("{0} request {1}\t\n{2}", corrId, requestInfo, body));
                }
                
                //response
                using var memStream = new MemoryStream();
                context.Response.Body = memStream;

                await _next(context);

                memStream.Position = 0;

                string responseBody = await new StreamReader(memStream).ReadToEndAsync();

                if (!requestInfo.Contains("swagger") || !responseBody.ToLower().Contains("swagger"))
                {
                    _logger.LogWarning(string.Format("{0} responseBody {1} {2}", corrId, requestInfo, responseBody));
                }

                memStream.Position = 0;
                await memStream.CopyToAsync(originalBody);

            }
            catch (Exception ex)
            {
                _logger.LogWarning(string.Format("{0} Exception {1} {2}", corrId, requestInfo, ex.Message.ToString()));
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
    }
}
