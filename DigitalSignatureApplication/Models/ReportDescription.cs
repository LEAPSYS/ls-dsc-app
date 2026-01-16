using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Models
{
    public class ReportDescription
    {
        public ReportDescription(string ReportFileName, Stream ReportStream)
        {
            this.ReportFileName = ReportFileName;
            this.ReportStream = ReportStream;
        }
        public string ReportFileName { get; set; }
        public Stream ReportStream { get; set; }
    }
}
