using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Models
{
    public class GeneratedReportDetails
    {
        public GeneratedReportDetails(byte[] PDFInBytes, string FileName, string DocumentTableName, string DocType,
            string AuthCompany, string signerName, string DocNum, string DatabaseName, string DocEntry, bool DSCShow = true,
            string reportPath = null, string OutputFolderName = null, string SignedQrText = null)
        {
            this.PDFInBytes = PDFInBytes;
            this.FileName = FileName;
            this.DocumentTableName = DocumentTableName;
            this.DocType = DocType;
            this.AuthCompany = AuthCompany;
            this.SignerName = signerName;
            this.DocNum = DocNum;
            this.DatabaseName = DatabaseName;
            this.DSCShow = DSCShow;
            this.DocEntry = DocEntry;
            ReportPath = reportPath;
            this.OutputFolderName = OutputFolderName;
            this.SignedQrText = SignedQrText;
        }
        public byte[] PDFInBytes { get; set; }
        public string FileName { get; set; }
        public string DocumentTableName { get; set; }
        public string DocType { get; set; }
        public string DocEntry { get; set; }
        public string DocNum { get; set; }
        public string AuthCompany { get; set; }
        public string SignerName { get; set; }
        public string DatabaseName { get; set; }
        public bool DSCShow { get; set; }
        public string ReportPath { get; set; }
        public string OutputFolderName { get; set; }
        public string SignedQrText { get; set; }
    }
}
