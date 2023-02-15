using System.Diagnostics;

namespace UPSMonitor.Server.Services
{
    public class WindowsShutdownService : IShutdownService
    {
        private const string ShutdownExecute = "shutdown.exe";
        private readonly ILogger<WindowsShutdownService> _logger;
        private readonly System.Timers.Timer _hibernateTimer = new();

        public WindowsShutdownService(ILogger<WindowsShutdownService> logger)
        {
            _logger = logger;
            _hibernateTimer.Elapsed += (sender, args) =>
            {
                _logger.LogInformation("WindowsShutdownService Hibernating.");
                Process.Start(ShutdownExecute, "/h");
            };
        }

        public void Shutdown(TimeSpan delay)
        {
            _logger.LogInformation("WindowsShutdownService Shutdown.");
            Process.Start(ShutdownExecute, $"/s /t {delay.TotalSeconds}");
        }

        public void Hibernate(TimeSpan delay)
        {
            _logger.LogInformation("WindowsShutdownService Hibernate scheduled.");
            _hibernateTimer.Interval = delay.TotalMilliseconds;
            _hibernateTimer.Start();
        }

        public void Cancel()
        {
            _logger.LogInformation("WindowsShutdownService Cancel.");
            if (_hibernateTimer.Enabled)
            {
                _hibernateTimer.Stop();
            }
            else
            {
                Process.Start(ShutdownExecute, "/a");
            }
        }
    }
}
