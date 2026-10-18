using System.Text;
using System.Text.Json;
using Irrigation.Application.Exceptions;
using Irrigation.Application.Interfaces.Messaging;
using Irrigation.Application.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace Irrigation.Infrastructure.Messaging;

public sealed class MqttPublisher : IMqttPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IMqttClient _client = new MqttFactory().CreateMqttClient();
    private readonly MqttOptions _options;
    private readonly ILogger<MqttPublisher> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public MqttPublisher(IOptions<MqttOptions> options, ILogger<MqttPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        _client.DisconnectedAsync += args =>
        {
            _logger.LogWarning(
                "Conexión MQTT perdida con {Host}:{Port} ({Reason})",
                _options.Host,
                _options.Port,
                args.Reason);
            return Task.CompletedTask;
        };
    }

    public async Task PublishStartCommandAsync(IrrigationCommandMessage command, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("MQTT deshabilitado: no se envió el comando de riego {ActionId}", command.ActionId);
            return;
        }

        var topic = $"{_options.CommandTopicPrefix}/{command.DeviceId}";
        await PublishAsync(topic, command, "comando de inicio de riego", cancellationToken);
    }

    public async Task PublishCancelCommandAsync(IrrigationCancelCommandMessage command, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("MQTT deshabilitado: no se envió el comando de cancelación {ActionId}", command.ActionId);
            return;
        }

        var topic = $"{_options.CancelTopicPrefix}/{command.DeviceId}";
        await PublishAsync(topic, command, "comando de cancelación de riego", cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected)
        {
            try
            {
                await _client.DisconnectAsync();
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Error al desconectar el cliente MQTT publisher");
            }
        }

        _client.Dispose();
        _connectionLock.Dispose();
    }

    private async Task PublishAsync<T>(string topic, T payload, string description, CancellationToken cancellationToken)
    {
        await EnsureConnectedAsync(cancellationToken);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.SerializeToUtf8Bytes(payload, SerializerOptions))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        var result = await _client.PublishAsync(message, cancellationToken);

        if (result.ReasonCode != MqttClientPublishReasonCode.Success)
        {
            throw new IrrigationApplicationException(
                $"El broker MQTT rechazó el {description} (código {result.ReasonCode}).");
        }

        _logger.LogInformation("Publicado {Description} en el tópico {Topic}", description, topic);
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_client.IsConnected)
        {
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_client.IsConnected)
            {
                return;
            }

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(_options.Host, _options.Port)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveSeconds));

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                optionsBuilder.WithCredentials(_options.Username, _options.Password ?? string.Empty);
            }

            await _client.ConnectAsync(optionsBuilder.Build(), cancellationToken);

            if (!_client.IsConnected)
            {
                throw new IrrigationApplicationException(
                    $"No se pudo establecer la conexión con el broker MQTT en {_options.Host}:{_options.Port}.");
            }

            _logger.LogInformation("Conectado al broker MQTT en {Host}:{Port}", _options.Host, _options.Port);
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}
