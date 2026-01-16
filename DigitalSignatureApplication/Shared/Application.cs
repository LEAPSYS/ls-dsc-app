using System.Threading.Tasks;
using System.Collections.Generic;
using DigitalSignatureApplication.Config;

namespace DigitalSignatureApplication.Shared
{
    public class Application
    {
        private readonly BulkSigningService bulkSigningService;
        private readonly CrystalReportService crystalReportService;
        public Application(BulkSigningService bsService, CrystalReportService crService)
        {
            crystalReportService = crService;
            this.bulkSigningService = bsService;
        }
        public async Task Sequence()
        {
            _ = await crystalReportService.GetGeneratedReportList();
            await this.bulkSigningService.ManualDSC();
        }
    }
}
