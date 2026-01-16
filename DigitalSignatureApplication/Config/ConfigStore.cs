using DigitalSignatureApplication.Models;

namespace DigitalSignatureApplication.Config
{
    static public class ConfigStore
    {
        public static LegacyPayload LegacyPayload { get; set; }
        public static ApiConfig ApiConfig { get; set; }
        public static ConnectionAndReportDetails ConnectionAndReportDetails { get; set; }
        public static CommonService CommonServices { get; set; }
    }
}
