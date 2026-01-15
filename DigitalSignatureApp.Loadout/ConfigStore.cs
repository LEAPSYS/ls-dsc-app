using DigitalSignatureApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Loadout
{
    static public class ConfigStore
    {
        public static DSCInfoAndAPI DSCInfoAndAPI { get; set; }
        public static ApiConfig ApiConfig { get; set; }
        public static ConnectionAndReportDetails ConnectionAndReportDetails { get; set; }
        public static CommonService CommonServices { get; set; }
    }
}
