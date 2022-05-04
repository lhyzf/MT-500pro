namespace UPSMonitor.Server.Services
{
    public interface IShutdownService
    {
        public void Shutdown(TimeSpan delay);
        public void Hibernate(TimeSpan delay);
        public void Cancel();

    }
}
