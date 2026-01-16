using DigitalSignatureApplication.Config;
using DigitalSignatureApplication.Models;
using DigitalSignatureApplication.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            ConfigurationLoad startup = new ConfigurationLoad();
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "LS.DSC.Service";
            });
            ConfigStore.LegacyPayload = builder.Configuration.GetSection("LegacyPayload").Get<LegacyPayload>();
            ConfigStore.ApiConfig = builder.Configuration.GetSection("ApiConfig").Get<ApiConfig>();
            builder.Services.AddHttpClient("signingAPI", (ServiceProvider, httpClient) =>
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", ConfigStore.ApiConfig.Auth);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.BaseAddress = new Uri(ConfigStore.ApiConfig.Url);
            });
            builder.Services.AddScoped<Application>();
            builder.Services.AddScoped<CrystalReportService>();
            builder.Services.AddTransient<BulkSigningService>();
            builder.Services.AddQuartz(quartz => quartz.AddJobAndTrigger<DigitalSignatureJob>(builder.Configuration));
            builder.Services.AddQuartzHostedService(quartz => quartz.WaitForJobsToComplete = true);
            builder.Logging.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/Program.log", rollingInterval: RollingInterval.Hour)
                .CreateLogger()
            );

            var host = builder.Build();
            host.Run();
            Log.Information("Program Started");
            Log.CloseAndFlush();
        }
    }
}
