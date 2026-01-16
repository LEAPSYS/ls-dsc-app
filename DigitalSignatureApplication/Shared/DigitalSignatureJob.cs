using DigitalSignatureApplication;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Shared
{
    [DisallowConcurrentExecution]
    public class DigitalSignatureJob : IJob
    {
        private readonly Application _app;
        public DigitalSignatureJob(Application app)
        {
            _app = app;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await _app.Sequence();
        }
    }
}
