namespace UPSMonitor.Server.Services
{
    public class UPSHostedService : IHostedService
    {
        private readonly ILogger<UPSHostedService> _logger;
        private readonly UPSService _upsService;

        public UPSHostedService(ILogger<UPSHostedService> logger, UPSService upsService)
        {
            _logger = logger;
            _upsService = upsService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("UPS Hosted Service running.");

            _upsService.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("UPS Hosted Service is stopping.");

            _upsService.Stop();

            return Task.CompletedTask;
        }
    }
}
