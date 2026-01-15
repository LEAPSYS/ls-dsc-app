using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DigitalSignatureApp.Loadout;
using DigitalSignatureApp.Models;
using DigitalSignatureApp;
using Quartz;

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
                options.ServiceName = "LS DSC Service";
            });
            ConfigStore.DSCInfoAndAPI = builder.Configuration.GetSection("Credentials").Get<DSCInfoAndAPI>();
            ConfigStore.ApiConfig = builder.Configuration.GetSection("ApiConfig").Get<ApiConfig>();
            builder.Services.AddHttpClient("signingAPI", (ServiceProvider, httpClient) =>
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", ConfigStore.ApiConfig.Auth);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.BaseAddress = new Uri(ConfigStore.ApiConfig.Url);
            });
            builder.Services.AddScoped<Application>();
            builder.Services.AddScoped<CrystalReport>();
            builder.Services.AddTransient<BulkDSC>();
            builder.Services.AddQuartz(q =>
            q.AddJobAndTrigger<DSCJob>(builder.Configuration)
            );
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            var host = builder.Build();
            host.Run();
           
        }
    }
}
