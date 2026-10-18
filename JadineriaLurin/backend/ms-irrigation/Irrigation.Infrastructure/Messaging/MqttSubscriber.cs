using System.Text;
using System.Text.Json;
using Irrigation.Application.Interfaces.Services;
using Irrigation.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace Irrigation.Infrastructure.Messaging;

public sealed class MqttSubscriber : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IMqttClient _client = new MqttFactory().CreateMqttClient();
    private readonly MqttOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttSubscriber> _logger;
    private CancellationToken _lifetimeToken = CancellationToken.None;

    public MqttSubscriber(
        IOptions<MqttOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<MqttSubscriber> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;

        _client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
        _client.DisconnectedAsync += OnDisconnectedAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _lifetimeToken = stoppingToken;

        if (!_options.Enabled)
        {
            _logger.LogWarning("Cliente MQTT deshabilitado: no se escucharán ACKs de dispositivos");
            return;
        }

        _logger.LogInformation(
            "Iniciando escucha MQTT en {Host}:{Port} con filtro '{Filter}'",
            _options.Host,
            _options.Port,
            _options.AckTopicFilter);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    await ConnectAndSubscribeAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Fallo en el ciclo de conexión MQTT del suscriptor");

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
                _logger.LogDebug(exception, "Error al desconectar el cliente MQTT suscriptor");
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
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveSeconds));

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            optionsBuilder.WithCredentials(_options.Username, _options.Password ?? string.Empty);
        }

        await _client.ConnectAsync(optionsBuilder.Build(), cancellationToken);

        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(filter => filter.WithTopic(_options.AckTopicFilter))
            .Build();

        await _client.SubscribeAsync(subscribeOptions, cancellationToken);

        _logger.LogInformation(
            "Suscrito al tópico '{Topic}' en {Host}:{Port}",
            _options.AckTopicFilter,
            _options.Host,
            _options.Port);
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning(
            "Suscriptor MQTT desconectado ({Reason}). Se reintentará la conexión",
            args.Reason);
        return Task.CompletedTask;
    }

    private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment.ToArray());
            var ack = JsonSerializer.Deserialize<IrrigationAckMessage>(payload, SerializerOptions);

            if (ack is null)
            {
                _logger.LogWarning("ACK MQTT vacío recibido en el tópico {Topic}", args.ApplicationMessage.Topic);
                return;
            }

            _logger.LogDebug(
                "ACK recibido para la acción {ActionId} con estado {Status}",
                ack.ActionId,
                ack.Status);

            using var scope = _scopeFactory.CreateScope();
            var irrigationService = scope.ServiceProvider.GetRequiredService<IIrrigationService>();
            await irrigationService.HandleDeviceAckAsync(ack, _lifetimeToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error al procesar el mensaje MQTT recibido en {Topic}",
                args.ApplicationMessage.Topic);
        }
    }
}
