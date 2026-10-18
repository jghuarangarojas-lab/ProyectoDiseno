using Iot.Application.DTOs.Requests;
using Iot.Application.DTOs.Responses;
using Iot.Application.Exceptions;
using Iot.Application.Interfaces.Realtime;
using Iot.Application.Interfaces.Repositories;
using Iot.Application.Interfaces.Services;
using Iot.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Iot.Application.Services;

public sealed class DeviceService : IDeviceService
{
    private const int DefaultMinHumidity = 30;
    private const int DefaultMaxHumidity = 70;
    private const double DefaultMinTemperature = 15d;
    private const double DefaultMaxTemperature = 30d;
    private const int DefaultAutoIrrigationSeconds = 30;

    private readonly IIotDeviceRepository _deviceRepository;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IIotStateNotifier _notifier;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(
        IIotDeviceRepository deviceRepository,
        ITelemetryRepository telemetryRepository,
        IIotStateNotifier notifier,
        TimeProvider timeProvider,
        ILogger<DeviceService> logger)
    {
        _deviceRepository = deviceRepository;
        _telemetryRepository = telemetryRepository;
        _notifier = notifier;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<IotDeviceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _deviceRepository.GetAllAsync(cancellationToken);
        return devices.Select(IotDeviceDto.From).ToList();
    }

    public async Task<IotDeviceDto> GetByCodeAsync(string deviceCode, CancellationToken cancellationToken = default)
    {
        var device = await GetRequiredDeviceAsync(deviceCode, cancellationToken);
        return IotDeviceDto.From(device);
    }

    public async Task<IotDeviceDto> RegisterAsync(CreateDeviceDto dto, CancellationToken cancellationToken = default)
    {
        var deviceCode = dto.DeviceCode.Trim();

        if (await _deviceRepository.GetByCodeAsync(deviceCode, cancellationToken) is not null)
        {
            throw new IotApplicationException($"Ya existe un dispositivo IoT registrado con código '{deviceCode}'.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var device = IotDevice.Create(deviceCode, dto.MacAddress, dto.CurrentSectorArea, utcNow);

        await _deviceRepository.AddAsync(device, cancellationToken);
        await _deviceRepository.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyDeviceUpdatedAsync(IotDeviceDto.From(device), cancellationToken);

        _logger.LogInformation(
            "Dispositivo '{DeviceCode}' registrado en el sector '{Sector}' con MAC {MacAddress}",
            device.DeviceCode,
            device.CurrentSectorArea,
            device.MacAddress);

        return IotDeviceDto.From(device);
    }

    public async Task<IotDeviceDto> RelocateAsync(
        string deviceCode,
        RelocateSensorDto dto,
        CancellationToken cancellationToken = default)
    {
        var device = await GetRequiredDeviceAsync(deviceCode, cancellationToken);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var previousSector = device.CurrentSectorArea;
        device.Relocate(dto.CurrentSectorArea, utcNow);
        await _deviceRepository.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyDeviceUpdatedAsync(IotDeviceDto.From(device), cancellationToken);

        _logger.LogInformation(
            "Sensor '{DeviceCode}' reubicado de '{PreviousSector}' a '{NewSector}'",
            device.DeviceCode,
            previousSector,
            device.CurrentSectorArea);

        return IotDeviceDto.From(device);
    }

    public async Task<BotanicalThresholdDto> GetThresholdAsync(
        string deviceCode,
        CancellationToken cancellationToken = default)
    {
        var device = await GetRequiredDeviceAsync(deviceCode, cancellationToken);

        if (device.Threshold is null)
        {
            throw new IotNotFoundException(
                $"El dispositivo '{deviceCode}' no tiene umbrales botánicos configurados.");
        }

        return BotanicalThresholdDto.From(device.Threshold);
    }

    public async Task<BotanicalThresholdDto> UpdateThresholdAsync(
        string deviceCode,
        UpdateThresholdDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.MinHumidityPercentage is null &&
            dto.MaxHumidityPercentage is null &&
            dto.MinTemperatureCelsius is null &&
            dto.MaxTemperatureCelsius is null &&
            dto.AutoIrrigationSeconds is null)
        {
            throw new IotApplicationException(
                "Debe indicar al menos un valor para actualizar los umbrales botánicos.");
        }

        var device = await GetRequiredDeviceAsync(deviceCode, cancellationToken);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var current = device.Threshold;

        var minHumidity = dto.MinHumidityPercentage ?? current?.MinHumidityPercentage ?? DefaultMinHumidity;
        var maxHumidity = dto.MaxHumidityPercentage ?? current?.MaxHumidityPercentage ?? DefaultMaxHumidity;
        var minTemperature = dto.MinTemperatureCelsius ?? current?.MinTemperatureCelsius ?? DefaultMinTemperature;
        var maxTemperature = dto.MaxTemperatureCelsius ?? current?.MaxTemperatureCelsius ?? DefaultMaxTemperature;
        var autoIrrigationSeconds = dto.AutoIrrigationSeconds ?? current?.AutoIrrigationSeconds ?? DefaultAutoIrrigationSeconds;

        if (current is null)
        {
            var threshold = BotanicalThreshold.Create(
                device.DeviceCode,
                minHumidity,
                maxHumidity,
                minTemperature,
                maxTemperature,
                autoIrrigationSeconds,
                utcNow);

            device.AssignThreshold(threshold, utcNow);
        }
        else
        {
            current.Update(minHumidity, maxHumidity, minTemperature, maxTemperature, autoIrrigationSeconds, utcNow);
            device.AssignThreshold(current, utcNow);
        }

        await _deviceRepository.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyDeviceUpdatedAsync(IotDeviceDto.From(device), cancellationToken);

        return BotanicalThresholdDto.From(device.Threshold!);
    }

    public async Task<IReadOnlyList<TelemetryRecordDto>> GetTelemetryAsync(
        string deviceCode,
        int limit,
        CancellationToken cancellationToken = default)
    {
        await GetRequiredDeviceAsync(deviceCode, cancellationToken);

        var normalizedLimit = limit <= 0
            ? IDeviceService.DefaultTelemetryLimit
            : Math.Min(limit, IDeviceService.MaxTelemetryLimit);

        var records = await _telemetryRepository.GetLastAsync(deviceCode, normalizedLimit, cancellationToken);
        return records.Select(TelemetryRecordDto.From).ToList();
    }

    private async Task<IotDevice> GetRequiredDeviceAsync(string deviceCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new IotApplicationException("El código del dispositivo es obligatorio.");
        }

        return await _deviceRepository.GetByCodeAsync(deviceCode.Trim(), cancellationToken)
            ?? throw new IotNotFoundException($"No existe ningún dispositivo IoT registrado con código '{deviceCode}'.");
    }
}
