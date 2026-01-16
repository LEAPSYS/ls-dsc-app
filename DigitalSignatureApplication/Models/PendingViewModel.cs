using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Models
{
    public class PendingViewModel
    {
        public string CREXPORT { get; set; }
        public string Type { get; set; }
        public string DocNum { get; set; }
        public string Database { get; set; }
        public string DocEntry { get; set; }
        public string TBName { get; set; }
        public string Auth_Signatory { get; set; }
        public string CRPath { get; set; }
        public bool DSCShow { get; set; }
        public string AuthorizedSignatory { get; set; }
        public string SignedQrText { get; set; }
        public PendingViewModel()
        {
            CREXPORT = string.Empty;
            Type = string.Empty;
            DocNum = string.Empty;
            Database = string.Empty;
            DocEntry = string.Empty;
            TBName = string.Empty;
            Auth_Signatory = string.Empty;
            CRPath = string.Empty;
            AuthorizedSignatory = string.Empty;
            SignedQrText = string.Empty;
        }

    }
}
