using Microsoft.Extensions.Configuration;
using System;
using DigitalSignatureApp.Models;
using DigitalSignatureApp;
using System.IO;

namespace DigitalSignatureApp.Loadout
{
    public class ConfigurationLoad
    {
        public ConfigurationLoad()
        {
            try
            {
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                // The location of our json files are passed to our builders first
                var CommonService_builder = new ConfigurationBuilder().SetBasePath(strWorkPath).AddJsonFile("commonsettings.json", optional: false);
                //var Dsc_settings_builder = new ConfigurationBuilder().SetBasePath(strWorkPath).AddJsonFile("appsettings.json", optional: false);
                var DataSource_builder = new ConfigurationBuilder().SetBasePath(strWorkPath).AddJsonFile("DataSourceAndReport.json", optional: false);
                // The config files are loaded after building from json files
                IConfiguration CommonConfig = CommonService_builder.Build();
                //IConfiguration DscConfig = Dsc_settings_builder.Build();
                IConfiguration DataSourceConfig = DataSource_builder.Build();
                //ConfigStore.DSCInfoAndAPI = DscConfig.GetSection("Credentials").Get<DSCInfoAndAPI>();
                //ConfigStore.ApiConfig = DscConfig.GetSection("ApiConfig").Get<ApiConfig>();
                ConfigStore.CommonServices = CommonConfig.GetSection("CommonService").Get<CommonService>();
                ConfigStore.ConnectionAndReportDetails = DataSourceConfig.GetSection("DataSourceConfig").Get<ConnectionAndReportDetails>();
            }
            catch(Exception ex)
            {
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                string logFolderPath = Path.Combine(strWorkPath, "ExceptionLog");
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string filePath = Path.Combine(logFolderPath, "Exception.txt");
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine();
                    while (ex != null)
                    {
                        writer.WriteLine(ex.GetType().FullName);
                        writer.WriteLine("Message : " + ex.Message);
                        writer.WriteLine("StackTrace : " + ex.StackTrace);
                        ex = ex.InnerException;
                    }
                }
            }
        }
    }
}
