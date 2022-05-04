using System.Text.Json.Serialization;

namespace UPSMonitor.Shared
{
    /// <summary>
    /// 状态
    /// </summary>
    public class UPSStatus
    {
        /// <summary>
        /// 命令字符串
        /// </summary>
        public const string Command = "Q1";
        /// <summary>
        /// 起始字符
        /// </summary>
        public const char StartChar = '(';
        /// <summary>
        /// 原始值
        /// </summary>
        public string? OriginValue { get; }
        /// <summary>
        /// 输入电压
        /// </summary>
        public string InVoltage { get; }
        /// <summary>
        /// 输入故障电压
        /// </summary>
        public string InFaultVoltage { get; }
        /// <summary>
        /// 输出电压
        /// </summary>
        public string OutVoltage { get; }
        /// <summary>
        /// 输出电流
        /// </summary>
        public string OutCurrent { get; }
        /// <summary>
        /// 输入频率
        /// </summary>
        public string InFrequency { get; }
        /// <summary>
        /// 电池电压
        /// </summary>
        public string BatteryVoltage { get; }
        /// <summary>
        /// 环境温度
        /// </summary>
        public string Temperature { get; }

        /// <summary>
        /// 市电电压异常
        /// </summary>
        public bool IsInVoltageAbnormal { get; }
        /// <summary>
        /// 电池低电压
        /// </summary>
        public bool IsBatteryVoltageLow { get; }
        /// <summary>
        /// Bypass 或 Buck Active
        /// </summary>
        public bool IsBypassOrBuckActive { get; }
        /// <summary>
        /// UPS 故障
        /// </summary>
        public bool IsUPSFault { get; }
        /// <summary>
        /// UPS 为后备式（否则表示在线式）
        /// </summary>
        public bool IsPassiveStandbyUPS { get; }
        /// <summary>
        /// 测试中
        /// </summary>
        public bool IsTesting { get; }
        /// <summary>
        /// 关机有效
        /// </summary>
        public bool IsPowerOff { get; }
        /// <summary>
        /// 蜂鸣器开
        /// </summary>
        public bool IsBuzzerOn { get; }

        [JsonConstructor]
        public UPSStatus(string inVoltage, string inFaultVoltage, string outVoltage, string outCurrent, string inFrequency, string batteryVoltage, string temperature, bool isInVoltageAbnormal, bool isBatteryVoltageLow, bool isBypassOrBuckActive, bool isUPSFault, bool isPassiveStandbyUPS, bool isTesting, bool isPowerOff, bool isBuzzerOn)
        {
            InVoltage = inVoltage;
            InFaultVoltage = inFaultVoltage;
            OutVoltage = outVoltage;
            OutCurrent = outCurrent;
            InFrequency = inFrequency;
            BatteryVoltage = batteryVoltage;
            Temperature = temperature;
            IsInVoltageAbnormal = isInVoltageAbnormal;
            IsBatteryVoltageLow = isBatteryVoltageLow;
            IsBypassOrBuckActive = isBypassOrBuckActive;
            IsUPSFault = isUPSFault;
            IsPassiveStandbyUPS = isPassiveStandbyUPS;
            IsTesting = isTesting;
            IsPowerOff = isPowerOff;
            IsBuzzerOn = isBuzzerOn;
        }

        public UPSStatus(string value)
        {
            OriginValue = value;
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            var startIndex = value.IndexOf(StartChar);
            if (startIndex < 0)
                throw new ArgumentException($"value must contains {StartChar}.", nameof(value));
            value = value[(startIndex + 1)..];
            var values = value.Split();
            InVoltage = values[0];
            InFaultVoltage = values[1];
            OutVoltage = values[2];
            OutCurrent = values[3];
            InFrequency = values[4];
            BatteryVoltage = values[5];
            Temperature = values[6];
            IsInVoltageAbnormal = values[7][0] == '1';
            IsBatteryVoltageLow = values[7][1] == '1';
            IsBypassOrBuckActive = values[7][2] == '1';
            IsUPSFault = values[7][3] == '1';
            IsPassiveStandbyUPS = values[7][4] == '1';
            IsTesting = values[7][5] == '1';
            IsPowerOff = values[7][6] == '1';
            IsBuzzerOn = values[7][7] == '1';
        }

        public override string ToString()
        {
            return OriginValue ?? $"{StartChar}{InVoltage} {InFaultVoltage} " +
                $"{OutVoltage} {OutCurrent} {InFrequency} {BatteryVoltage} {Temperature} " +
                $"{(IsInVoltageAbnormal ? '1' : '0')} {(IsBatteryVoltageLow ? '1' : '0')} " +
                $"{(IsBypassOrBuckActive ? '1' : '0')} {(IsUPSFault ? '1' : '0')} " +
                $"{(IsPassiveStandbyUPS ? '1' : '0')} {(IsTesting ? '1' : '0')} " +
                $"{(IsPowerOff ? '1' : '0')} {(IsBuzzerOn ? '1' : '0')}";
        }
    }
}
