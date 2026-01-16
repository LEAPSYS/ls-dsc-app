using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.Shared
{
    [DisallowConcurrentExecution]
    public class DigitalSignatureJob : IJob
    {
        private readonly Application _app;
        private readonly ILogger<DigitalSignatureJob> _logger;
        public DigitalSignatureJob(Application app, ILogger<DigitalSignatureJob> logger)
        {
            this._logger = logger;
            this._app = app;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Execute called");
            await _app.Sequence();
        }
    }
}
