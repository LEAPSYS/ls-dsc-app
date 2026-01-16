using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Models
{
    public class DigiSignResponse
    {
        public string b64Signed { get; set; }
        public string urlSigned { get; set; }
    }
}
