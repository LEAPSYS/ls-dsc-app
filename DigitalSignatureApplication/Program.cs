using DigitalSignatureApplication.Config;
using DigitalSignatureApplication.Models;
using DigitalSignatureApplication.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;
using System;

namespace DigitalSignatureApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                ConfigurationLoad startup = new ConfigurationLoad();

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File("logs/Program.log", rollingInterval: RollingInterval.Hour)
                    .CreateLogger();

                Log.Information("Service bootstrapping started");

                var builder = Host.CreateApplicationBuilder(args);

                builder.Logging.ClearProviders();
                builder.Logging.AddSerilog(Log.Logger);

                builder.Services.AddWindowsService(options =>
                {
                    options.ServiceName = "LS.DSC.Service";
                });

                ConfigStore.LegacyPayload =
                    builder.Configuration.GetSection("LegacyPayload").Get<LegacyPayload>();

                ConfigStore.ApiConfig =
                    builder.Configuration.GetSection("ApiConfig").Get<ApiConfig>();

                builder.Services.AddHttpClient("signingAPI", (sp, httpClient) =>
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", ConfigStore.ApiConfig.Auth);
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.BaseAddress = new Uri(ConfigStore.ApiConfig.Url);
                });

                builder.Services.AddScoped<Application>();
                builder.Services.AddScoped<CrystalReportService>();
                builder.Services.AddTransient<BulkSigningService>();

                builder.Services.AddQuartz(q =>
                    q.AddJobAndTrigger<DigitalSignatureJob>(builder.Configuration));

                builder.Services.AddQuartzHostedService(q =>
                    q.WaitForJobsToComplete = true);

                var host = builder.Build();

                Log.Information("Service starting...");
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Service terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
