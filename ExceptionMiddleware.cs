using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SerilogSample
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private ILogger logger;
        public ExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, ILogger<ExceptionMiddleware> logger)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException vex)
            {
                await ReturnExceptionStatus(context, HttpStatusCode.NonAuthoritativeInformation, vex.Message);
            }
            catch (Exception ex)
            {
                this.logger = logger;
                await ReturnExceptionStatus(context, HttpStatusCode.InternalServerError, "Unexpected error");
                await SaveExceptionLog(context, ex);
            }
        }

        private async Task ReturnExceptionStatus<T>(HttpContext context, HttpStatusCode httpStatusCode, T result)
        {
            context.Response.StatusCode = (int)httpStatusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            var response = result;
            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }

        private async Task SaveExceptionLog(HttpContext context, Exception ex)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync();
            var requestHeaders = context.Request.Headers.Aggregate(new StringBuilder(),
               (a, b) => a.AppendFormat("{0}:{1} ", a.Length > 0 ? b.Key : string.Empty, b.Value), sb => sb.ToString());
            logger.LogError(string.Format("Stacktrace:{0}, RequestPath:{1}, RequestBody:{2}, RequestHeaders:{3},",
                ex.StackTrace,context.Request.Path,requestBody,requestHeaders));
        }
    }
}
