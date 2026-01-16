using System.Threading.Tasks;
using System.Collections.Generic;
using DigitalSignatureApplication.Config;

namespace DigitalSignatureApplication.Shared
{
    public class Application
    {
        private readonly BulkSigningService bulkDSC;
        private readonly CrystalReportService crystalReport;
        public Application(BulkSigningService bulkDSC, CrystalReportService NewReport)
        {
            crystalReport = NewReport;
            this.bulkDSC = bulkDSC;
        }
        public async Task Sequence()
        {
            _ = await crystalReport.Report();
            await this.bulkDSC.ManualDSC();
        }
    }
}
