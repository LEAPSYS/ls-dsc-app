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
    public class DSCJob : IJob
    {
        private readonly Application _app;
        public DSCJob(Application app)
        {
            _app = app;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await _app.Sequence();
        }
    }
}
