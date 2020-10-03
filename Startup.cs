using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace SerilogSample
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    string result;
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature?.Error is ValidationException vex)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;
                        result = vex.Message;
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        result = "Unexpected error";
                    }
                    context.Response.ContentType = "application/json";
                    var response = result;
                    var json = JsonSerializer.Serialize(response);
                });
            });
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = async (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHeaders", httpContext.Request.Headers);
                    using var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8);
                    var body = await reader.ReadToEndAsync();
                    diagnosticContext.Set("RequestBody", body);
                };
            });
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
