using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Models
{
    public class CommonService
    {
        public bool TurnOnExceptionLog { get; set; }
        public bool StepByStepEvaluation { get; set; }
        public bool WriteAtEverySuccessOrFail { get; set; }
        public string DataSource { get; set; }
        public ReportGenerationMode ReportGenerationSettings { get; set; }
    }
    public class ReportGenerationMode
    {
        public bool DSCService { get; set; }
        public bool MailService { get; set; }
        public MailServiceDetails MailServiceDetails { get; set; }

    }
    public class MailServiceDetails
    {

        public List<FromField> FromFields { get; set; }
        public List<ToField> ToFields { get; set; }
        public List<CCField> CCFields { get; set; }
        public List<BCCField> BCCFields { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }

    }
    public class FromField
    {
        public string FromName { get; set; }
        public string FromAddress { get; set; }
    }
    public class ToField
    {
        public string ToName { get; set; }
        public string ToAddress { get; set; }
    }
    public class CCField
    {
        public string ToName { get; set; }
        public string ToAddress { get; set; }
    }
    public class BCCField
    {
        public string ToName { get; set; }
        public string ToAddress { get; set; }
    }

}
