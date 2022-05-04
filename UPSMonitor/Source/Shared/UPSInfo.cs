using System.Text.Json.Serialization;

namespace UPSMonitor.Shared
{
    /// <summary>
    /// 额定值信息
    /// </summary>
    public class UPSInfo
    {
        /// <summary>
        /// 命令字符串
        /// </summary>
        public const string Command = "F";
        /// <summary>
        /// 起始字符
        /// </summary>
        public const char StartChar = '#';
        /// <summary>
        /// 原始值
        /// </summary>
        public string? OriginValue { get; }
        /// <summary>
        /// 额定电压
        /// </summary>
        public string Voltage { get; }
        /// <summary>
        /// 额定电流
        /// </summary>
        public string Current { get; }
        /// <summary>
        /// 电池电压
        /// </summary>
        public string BatteryVoltage { get; }
        /// <summary>
        /// 额定频率
        /// </summary>
        public string Frequency { get; }

        [JsonConstructor]
        public UPSInfo(string voltage, string current, string batteryVoltage, string frequency)
        {
            Voltage = voltage;
            Current = current;
            BatteryVoltage = batteryVoltage;
            Frequency = frequency;
        }

        public UPSInfo(string value)
        {
            OriginValue = value;
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            var startIndex = value.IndexOf(StartChar);
            if (startIndex < 0)
                throw new ArgumentException($"value must contains {StartChar}.", nameof(value));
            value = value[(startIndex + 1)..];
            var values = value.Split();
            Voltage = values[0];
            Current = values[1];
            BatteryVoltage = values[2];
            Frequency = values[3];
        }

        public override string ToString()
        {
            return OriginValue ?? $"{StartChar}{Voltage} {Current} {BatteryVoltage} {Frequency}";
        }
    }
}
