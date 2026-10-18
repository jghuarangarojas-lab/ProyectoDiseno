using Irrigation.Application.DTOs.Requests;
using Irrigation.Application.DTOs.Responses;
using Irrigation.Application.Exceptions;
using Irrigation.Application.Interfaces.Messaging;
using Irrigation.Application.Interfaces.Realtime;
using Irrigation.Application.Interfaces.Repositories;
using Irrigation.Application.Interfaces.Services;
using Irrigation.Application.Messaging;
using Irrigation.Domain.Entities;
using Irrigation.Domain.Enums;
using Irrigation.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Irrigation.Application.Services;

public sealed class IrrigationService : IIrrigationService
{
    private const int DefaultListLimit = 20;
    private const int MaxListLimit = 100;

    private readonly IIrrigationRepository _repository;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly IIrrigationStateNotifier _stateNotifier;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<IrrigationService> _logger;

    public IrrigationService(
        IIrrigationRepository repository,
        IMqttPublisher mqttPublisher,
        IIrrigationStateNotifier stateNotifier,
        TimeProvider timeProvider,
        ILogger<IrrigationService> logger)
    {
        _repository = repository;
        _mqttPublisher = mqttPublisher;
        _stateNotifier = stateNotifier;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<IrrigationActionDto> StartAsync(StartIrrigationDto dto, CancellationToken cancellationToken = default)
    {
        var triggerType = ParseTriggerType(dto.TriggerType);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        if (triggerType is TriggerType.MANUAL && dto.RequestedDurationSeconds > IrrigationAction.PumpDurationLimitSeconds)
        {
            throw new IrrigationDomainException(
                $"La duración manual no puede exceder los {IrrigationAction.PumpDurationLimitSeconds} segundos.");
        }

        var action = IrrigationAction.Create(
            dto.DeviceId,
            dto.SectorArea,
            triggerType,
            dto.RequestedDurationSeconds,
            utcNow);

        await _repository.AddAsync(action, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var command = new IrrigationCommandMessage(
            action.Id,
            action.DeviceId,
            action.SectorArea,
            action.RequestedDurationSeconds,
            action.MaxPumpDurationSeconds,
            action.TriggerType.ToString(),
            utcNow);

        try
        {
            await _mqttPublisher.PublishStartCommandAsync(command, cancellationToken);
        }
        catch (Exception exception)
        {
            action.Fail($"No se pudo publicar el comando MQTT: {exception.Message}", utcNow);
            await _repository.SaveChangesAsync(cancellationToken);
            await NotifyAsync(action, cancellationToken);

            throw new IrrigationApplicationException(
                "La solicitud de riego se registró pero no se pudo enviar el comando al dispositivo.",
                exception);
        }

        _logger.LogInformation(
            "Riego {ActionId} iniciado para el dispositivo {DeviceId} en el sector {SectorArea} ({Duration}s)",
            action.Id,
            action.DeviceId,
            action.SectorArea,
            action.RequestedDurationSeconds);

        await NotifyAsync(action, cancellationToken);
        return IrrigationActionDto.From(action);
    }

    public async Task<IrrigationActionDto> CancelAsync(Guid irrigationActionId, CancelIrrigationDto dto, CancellationToken cancellationToken = default)
    {
        var action = await _repository.GetByIdAsync(irrigationActionId, cancellationToken)
            ?? throw new IrrigationNotFoundException($"No existe ninguna acción de riego con id '{irrigationActionId}'.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        if (action.RequiresDeviceCancellation)
        {
            var command = new IrrigationCancelCommandMessage(action.Id, action.DeviceId, dto.Reason, utcNow);

            try
            {
                await _mqttPublisher.PublishCancelCommandAsync(command, cancellationToken);
            }
            catch (Exception exception)
            {
                throw new IrrigationApplicationException(
                    "No se pudo enviar la orden de cancelación al dispositivo. La acción no fue cancelada.",
                    exception);
            }
        }

        action.Cancel(dto.Reason, utcNow);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Riego {ActionId} cancelado: {Reason}", action.Id, action.CancellationReason);

        await NotifyAsync(action, cancellationToken);
        return IrrigationActionDto.From(action);
    }

    public async Task<IrrigationActionDto> UpdateCancellationReasonAsync(Guid irrigationActionId, UpdateCancellationReasonDto dto, CancellationToken cancellationToken = default)
    {
        var action = await _repository.GetByIdAsync(irrigationActionId, cancellationToken)
            ?? throw new IrrigationNotFoundException($"No existe ninguna acción de riego con id '{irrigationActionId}'.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        action.UpdateCancellationReason(dto.Reason, utcNow);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Motivo de cancelación actualizado para la acción {ActionId}: {Reason}",
            action.Id,
            action.CancellationReason);

        await NotifyAsync(action, cancellationToken);
        return IrrigationActionDto.From(action);
    }

    public async Task<IrrigationActionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var action = await _repository.GetByIdAsync(id, cancellationToken);
        return action is null ? null : IrrigationActionDto.From(action);
    }

    public async Task<IReadOnlyList<IrrigationActionDto>> GetRecentByDeviceAsync(string deviceId, int limit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new IrrigationApplicationException("DeviceId es obligatorio para consultar el historial de riego.");
        }

        var effectiveLimit = limit <= 0 ? DefaultListLimit : Math.Min(limit, MaxListLimit);
        var actions = await _repository.ListRecentByDeviceAsync(deviceId.Trim(), effectiveLimit, cancellationToken);

        return actions.Select(IrrigationActionDto.From).ToList();
    }

    public async Task<IrrigationActionDto?> HandleDeviceAckAsync(IrrigationAckMessage ack, CancellationToken cancellationToken = default)
    {
        var action = await _repository.GetByIdAsync(ack.ActionId, cancellationToken);

        if (action is null)
        {
            _logger.LogWarning("Se recibió un ACK para la acción desconocida {ActionId}", ack.ActionId);
            return null;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var status = (ack.Status ?? string.Empty).Trim().ToUpperInvariant();

        try
        {
            switch (status)
            {
                case "ACK":
                case "ACCEPTED":
                case "PENDING":
                    return IrrigationActionDto.From(action);

                case "IN_PROGRESS":
                case "STARTED":
                    action.MarkAsInProgress(utcNow);
                    break;

                case "COMPLETED":
                case "FINISHED":
                    if (action.Status is IrrigationStatus.PENDING)
                    {
                        action.MarkAsInProgress(utcNow);
                    }

                    action.Complete(ack.ActualDurationSeconds ?? action.RequestedDurationSeconds, utcNow);
                    break;

                case "FAILED":
                    action.Fail(ack.Reason ?? "Falla reportada por el dispositivo.", utcNow);
                    break;

                case "CANCELLED":
                    action.Cancel(ack.Reason ?? "Cancelación confirmada por el dispositivo.", utcNow);
                    break;

                default:
                    _logger.LogWarning(
                        "Estado ACK no reconocido '{Status}' para la acción {ActionId}",
                        ack.Status,
                        ack.ActionId);
                    return IrrigationActionDto.From(action);
            }
        }
        catch (IrrigationDomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Transición de estado inválida al procesar el ACK de la acción {ActionId}",
                ack.ActionId);
            return IrrigationActionDto.From(action);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        await NotifyAsync(action, cancellationToken);

        _logger.LogInformation("Acción {ActionId} actualizada a {Status} por ACK del dispositivo", action.Id, action.Status);

        return IrrigationActionDto.From(action);
    }

    private async Task NotifyAsync(IrrigationAction action, CancellationToken cancellationToken)
    {
        try
        {
            await _stateNotifier.NotifyStateChangedAsync(IrrigationActionDto.From(action), cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "No se pudo notificar el cambio de estado de la acción {ActionId}", action.Id);
        }
    }

    private static TriggerType ParseTriggerType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return TriggerType.MANUAL;
        }

        if (Enum.TryParse<TriggerType>(value.Trim(), ignoreCase: true, out var triggerType))
        {
            return triggerType;
        }

        throw new IrrigationApplicationException($"TriggerType '{value}' no es válido. Valores permitidos: AUTOMATIC, MANUAL.");
    }
}
