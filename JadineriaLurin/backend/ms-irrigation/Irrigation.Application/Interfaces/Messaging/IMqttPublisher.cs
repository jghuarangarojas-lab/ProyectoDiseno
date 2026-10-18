using Irrigation.Application.Messaging;

namespace Irrigation.Application.Interfaces.Messaging;

public interface IMqttPublisher
{
    Task PublishStartCommandAsync(IrrigationCommandMessage command, CancellationToken cancellationToken = default);

    Task PublishCancelCommandAsync(IrrigationCancelCommandMessage command, CancellationToken cancellationToken = default);
}
