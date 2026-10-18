namespace Irrigation.Infrastructure.Messaging;

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
    public string CommandTopicPrefix { get; set; } = "irrigation/cmd";
    public string CancelTopicPrefix { get; set; } = "irrigation/cancel";
    public string AckTopicFilter { get; set; } = "irrigation/ack/+";
}
