using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Models
{
    public class ConnectionAndReportDetails
    {
        public ConnectionSettings ConnectionDetails { get; set; }
        public LoginDetails loginDetails { get; set; }
        public ReportDesc reportInformation { get; set; }
    }
    public class ConnectionSettings
    {
        public string Connection_String { get; set; }
        public string providerName { get; set; }
    }
    public class LoginDetails
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string userId { get; set; }
        public string password { get; set; }
    }
    public class ReportDesc
    {
        public string DataQuery { get; set; }
        public string crystalReportLocation { get; set; }
        public string exportLocation { get; set; }
    }
}
