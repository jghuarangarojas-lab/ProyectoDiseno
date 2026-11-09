namespace Iot.Infrastructure.Messaging;

public sealed class MqttOptions
{
    public const string SectionName = "Mqtt";

    public bool Enabled { get; set; } = true;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int KeepAliveSeconds { get; set; } = 60;
    public int ReconnectDelaySeconds { get; set; } = 5;
    public string ClientId { get; set; } = "ms-iot";
    public string TelemetryTopicFilter { get; set; } = "iot/telemetry/+";
    public string LastWillTopicFilter { get; set; } = "iot/lastwill/+";
    public int SweepIntervalSeconds { get; set; } = 300;
    public int StaleDeviceSeconds { get; set; } = 300;
}
