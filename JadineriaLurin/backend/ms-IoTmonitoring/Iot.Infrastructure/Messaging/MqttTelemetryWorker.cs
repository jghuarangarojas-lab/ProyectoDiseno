using System.Text;
using System.Text.Json;
using Iot.Application.DTOs.Requests;
using Iot.Application.Interfaces.Repositories;
using Iot.Application.Interfaces.Services;
using Iot.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace Iot.Infrastructure.Messaging;

public sealed class MqttTelemetryWorker : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IMqttClient _client = new MqttFactory().CreateMqttClient();
    private readonly MqttOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<MqttTelemetryWorker> _logger;
    private CancellationToken _lifetimeToken = CancellationToken.None;

    public MqttTelemetryWorker(
        IOptions<MqttOptions> options,
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<MqttTelemetryWorker> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;

        _client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
        _client.DisconnectedAsync += OnDisconnectedAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _lifetimeToken = stoppingToken;

        if (!_options.Enabled)
        {
            _logger.LogWarning("Cliente MQTT deshabilitado: no se escuchará telemetría ni Last Will");
            return;
        }

        _logger.LogInformation(
            "Iniciando worker MQTT de telemetría en {Host}:{Port} (tópicos '{Telemetry}' y '{LastWill}'), " +
            "barrido de dispositivos caídos cada {Sweep}s",
            _options.Host,
            _options.Port,
            _options.TelemetryTopicFilter,
            _options.LastWillTopicFilter,
            _options.SweepIntervalSeconds);

        var sweepInterval = TimeSpan.FromSeconds(_options.SweepIntervalSeconds);
        var nextSweepUtc = _timeProvider.GetUtcNow().UtcDateTime + sweepInterval;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    await ConnectAndSubscribeAsync(stoppingToken);
                }

                if (_timeProvider.GetUtcNow().UtcDateTime >= nextSweepUtc)
                {
                    nextSweepUtc = _timeProvider.GetUtcNow().UtcDateTime + sweepInterval;
                    await SweepStaleDevicesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Fallo en el ciclo de conexión MQTT del worker de telemetría");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.ReconnectDelaySeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        if (_client.IsConnected)
        {
            try
            {
                await _client.DisconnectAsync();
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Error al desconectar el cliente MQTT del worker de telemetría");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.ApplicationMessageReceivedAsync -= OnApplicationMessageReceivedAsync;
        _client.DisconnectedAsync -= OnDisconnectedAsync;
        await base.StopAsync(cancellationToken);
    }

    private async Task ConnectAndSubscribeAsync(CancellationToken cancellationToken)
    {
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId(_options.ClientId)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveSeconds));

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            optionsBuilder.WithCredentials(_options.Username, _options.Password ?? string.Empty);
        }

        await _client.ConnectAsync(optionsBuilder.Build(), cancellationToken);

        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(filter => filter.WithTopic(_options.TelemetryTopicFilter))
            .WithTopicFilter(filter => filter.WithTopic(_options.LastWillTopicFilter))
            .Build();

        await _client.SubscribeAsync(subscribeOptions, cancellationToken);

        _logger.LogInformation(
            "Suscrito a los tópicos '{Telemetry}' y '{LastWill}' en {Host}:{Port}",
            _options.TelemetryTopicFilter,
            _options.LastWillTopicFilter,
            _options.Host,
            _options.Port);
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning(
            "Worker MQTT de telemetría desconectado ({Reason}). Se reintentará la conexión",
            args.Reason);
        return Task.CompletedTask;
    }

    private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment.ToArray());

            if (IsLastWillTopic(topic))
            {
                await HandleLastWillAsync(topic, payload);
                return;
            }

            await HandleTelemetryAsync(topic, payload);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error al procesar el mensaje MQTT recibido en {Topic}",
                args.ApplicationMessage.Topic);
        }
    }

    private async Task HandleTelemetryAsync(string topic, string payload)
    {
        var message = DeserializeOrDefault<TelemetryMessage>(payload);
        var deviceCode = !string.IsNullOrWhiteSpace(message?.DeviceCode)
            ? message.DeviceCode
            : ExtractDeviceCodeFromTopic(topic);

        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            _logger.LogWarning("Telemetría sin DeviceCode recibida en el tópico {Topic}", topic);
            return;
        }

        if (message is null)
        {
            _logger.LogWarning("Payload de telemetría inválido en el tópico {Topic}", topic);
            return;
        }

        var input = new TelemetryInputDto
        {
            DeviceCode = deviceCode,
            HumidityPercentage = message.HumidityPercentage,
            TemperatureCelsius = message.TemperatureCelsius,
            BatteryPercentage = message.BatteryPercentage,
            SentAtUtc = message.SentAtUtc
        };

        using var scope = _scopeFactory.CreateScope();
        var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
        var device = await telemetryService.ProcessTelemetryAsync(input, _lifetimeToken);

        _logger.LogDebug(
            "Telemetría de '{DeviceCode}' procesada: {Humidity}% humedad, {Temperature}°C, batería {Battery}%",
            device.DeviceCode,
            input.HumidityPercentage,
            input.TemperatureCelsius,
            input.BatteryPercentage);
    }

    private async Task HandleLastWillAsync(string topic, string payload)
    {
        var message = DeserializeOrDefault<LastWillMessage>(payload);
        var deviceCode = !string.IsNullOrWhiteSpace(message?.DeviceCode)
            ? message.DeviceCode
            : ExtractDeviceCodeFromTopic(topic);

        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            _logger.LogWarning("Last Will sin DeviceCode recibido en el tópico {Topic}", topic);
            return;
        }

        var reason = string.IsNullOrWhiteSpace(message?.Reason)
            ? $"Last Will recibido en el tópico '{topic}' (el dispositivo cerró la conexión MQTT de forma abrupta)"
            : message.Reason;

        using var scope = _scopeFactory.CreateScope();
        var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
        var device = await telemetryService.HandleDeviceUnavailableAsync(deviceCode, reason, _lifetimeToken);

        if (device is not null)
        {
            _logger.LogWarning(
                "Last Will procesado para '{DeviceCode}': {Reason}",
                device.DeviceCode,
                reason);
        }
    }

    private async Task SweepStaleDevicesAsync(CancellationToken cancellationToken)
    {
        var cutoffUtc = _timeProvider.GetUtcNow().UtcDateTime - TimeSpan.FromSeconds(_options.StaleDeviceSeconds);

        using var scope = _scopeFactory.CreateScope();
        var deviceRepository = scope.ServiceProvider.GetRequiredService<IIotDeviceRepository>();
        var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();

        var staleDevices = await deviceRepository.GetStaleAsync(cutoffUtc, cancellationToken);

        foreach (var device in staleDevices)
        {
            var reason =
                $"Sin telemetría desde {device.LastSeenAtUtc:u} (silencio mayor a {_options.StaleDeviceSeconds}s)";

            await telemetryService.HandleDeviceUnavailableAsync(device.DeviceCode, reason, cancellationToken);
        }

        if (staleDevices.Count > 0)
        {
            _logger.LogWarning(
                "Barrido periódico ({SweepInterval}s): {Count} dispositivos marcados como desconectados",
                _options.SweepIntervalSeconds,
                staleDevices.Count);
        }
    }

    private bool IsLastWillTopic(string topic)
    {
        var prefix = _options.LastWillTopicFilter.Replace("+", string.Empty).TrimEnd('/');

        if (!string.IsNullOrWhiteSpace(prefix) &&
            topic.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return topic.Contains("/lastwill/", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractDeviceCodeFromTopic(string topic)
    {
        var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return segments.Length == 0 ? null : segments[^1];
    }

    private static T? DeserializeOrDefault<T>(string payload)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(payload, SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
