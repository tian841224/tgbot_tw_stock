using TGBot_TW_Stock_Polling.Interface;

namespace TGBot_TW_Stock_Polling.Services
{
    public class InitService : IHostedService
    {
        private readonly ILogger<InitService> _logger;
        private readonly IBrowserHandlers _browserHandlers;

        public InitService(ILogger<InitService> logger, IBrowserHandlers browserHandlers)
        {
            _logger = logger;
            _browserHandlers = browserHandlers;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _browserHandlers.LunchesPlaywright();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
