using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Models
{
    public class PayloadV3
    {
        public string orgCode { get; set; }
        public string authorityName { get; set; }
        public string b64Pdf { get; set; }
        public string placeholderText { get; set; }
        public bool url { get; set; }
        public int xAxisOffset { get; set; }
        public int yAxisOffset { get; set; }
        public int[] pages { get; set; }
        public string signedQrText { get; set; }
    }
}
