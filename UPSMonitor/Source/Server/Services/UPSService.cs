#if DEBUG
#define FAKE_COM_DATA
#endif

using System.IO.Ports;
using UPSMonitor.Shared;

namespace UPSMonitor.Server.Services
{
    public class UPSService : IDisposable
    {
        private readonly ILogger<UPSService> _logger;
        private readonly IShutdownService _shutdownService;
        private readonly SerialPort _serialPort = new();
        private readonly Thread _readThread;
        public UPSInfo? Info { get; private set; }
        public UPSStatus? Status { get; private set; }
        private bool _continue;
        private bool _isShutdownPlanned;
        public TimeSpan ReadSerialPortInterval { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan ShutdownDelay { get; set; } = TimeSpan.FromMinutes(1);

        public UPSService(ILogger<UPSService> logger, IShutdownService shutdownService)
        {
            _logger = logger;
            _shutdownService = shutdownService;
            _readThread = new Thread(Read);
        }

        public async void Start()
        {
            _logger.LogInformation("UPSService Service running.");
#if FAKE_COM_DATA
            Info = new UPSInfo("#220.0 004 12.00 50.0");
#else
            var portNames = GetPortNames();
            if (portNames.Length > 0)
            {
                var portName = portNames.First();
                OpenPort(portName);
            }
            else
            {
                _logger.LogWarning("No availble port.");
                return;
            }
            while (Info == null)
            {
                try
                {
                    _serialPort.WriteLine(UPSInfo.Command);
                    Info = new UPSInfo(_serialPort.ReadLine());
                }
                catch (Exception ex)
                {
                    _logger.LogError("UPSService start reading serial port({_serialPort.PortName}) error({ex.Message}).", _serialPort.PortName, ex.Message);
                    await Task.Delay(ReadSerialPortInterval);
                }
            }
#endif
            _logger.LogInformation("{Info}", Info);
            _continue = true;
            _readThread.Start();
        }

        public void Stop()
        {
            _logger.LogInformation("UPSService is stopping.");
            _continue = false;
            _readThread.Join();
        }

        public async void Read()
        {
            while (_continue)
            {
                try
                {
#if FAKE_COM_DATA
                    Status = new UPSStatus("(220.4 220.8 220.8 021 50.0 13.1 25.0 00001001");
#else
                    _serialPort.WriteLine(UPSStatus.Command);
                    string message = _serialPort.ReadLine();
                    Status = new UPSStatus(message);
#endif
                    _logger.LogInformation("{Status}", Status);
                    if (Status.IsInVoltageAbnormal)
                    {
                        if (!_isShutdownPlanned)
                        {
                            _shutdownService.Hibernate(ShutdownDelay);
                            _isShutdownPlanned = true;
                        }
                    }
                    else if (_isShutdownPlanned)
                    {
                        _shutdownService.Cancel();
                        _isShutdownPlanned = false;
                    }
                    await Task.Delay(ReadSerialPortInterval);
                }
                catch (TimeoutException ex)
                {
                    _logger.LogError("UPSService Read() timeout({ex.Message}).", ex.Message);
                }
            }
        }

        public void OpenPort(string portName)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _logger.LogInformation("UPSService OpenPort({portName}).", portName);
            // Allow the user to set the appropriate properties.
            _serialPort.PortName = portName;
            _serialPort.BaudRate = 2400;
            _serialPort.NewLine = "\r";
            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();
        }

        public string[] GetPortNames() => SerialPort.GetPortNames();
        public string GetPortName() => _serialPort.PortName;



        #region IDisposable

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // 释放托管状态(托管对象)
                    _serialPort?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~UPSService()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
