using System.Threading.Tasks;
using System.Collections.Generic;
using DigitalSignatureApp.Loadout;

namespace DigitalSignatureApp
{
    public class Application
    {
        private readonly BulkDSC bulkDSC;
        private readonly CrystalReport crystalReport;
        public Application(BulkDSC bulkDSC, CrystalReport NewReport)
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
