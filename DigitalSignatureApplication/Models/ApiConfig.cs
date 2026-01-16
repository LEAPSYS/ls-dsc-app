using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Models
{
    public class ApiConfig
    {
        public string Url { get; set; }
        public string Auth { get; set; }
        public string DSCInLocation { get; set; }
        public string DSCOutLocation { get; set; }
        public bool UsePayloadV3 { get; set; }
    }
}
