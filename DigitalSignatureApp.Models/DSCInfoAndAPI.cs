using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Models
{
    public class DSCInfoAndAPI
    {
        public string pdfByte1 { get; set; }

        public string AuthorizedSignatory { get; set; }

        public string SignerName { get; set; }
        public int TopLeft { get; set; }
        public int BottomLeft { get; set; }
        public int TopRight { get; set; }
        public int BottomRight { get; set; }
        public string ExcludePageNo { get; set; }
        public string InvoiceNumber { get; set; }
        public int pageNo { get; set; }
        public string PrintDateTime { get; set; }
        public string FindAuth { get; set; }
        public int FindAuthLocation { get; set; }
        public int fontsize { get; set; }
        public int adjustCoordinates { get; set; }
        public int signOnlySearchTextPage { get; set; }
    }
    public class ApiConfig
    {
        public string Url { get; set; }
        public string Auth { get; set; }
        public string DSCInLocation { get; set; }
        public string DSCOutLocation { get; set; }
    }
}
