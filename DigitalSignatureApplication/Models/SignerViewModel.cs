using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Models
{
    public class SignerViewModel
    {
        public string SignerName { get; set; }
        public string SignerTextSearch { get; set; }
        public int TopLeft { get; set; }
    }
}
