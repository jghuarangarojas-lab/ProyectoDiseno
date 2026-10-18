namespace Iot.Application.Interfaces.External;

public sealed record IrrigationRequest(
    string DeviceId,
    string SectorArea,
    int DurationSeconds);

public sealed record IrrigationRequestResult(
    bool Succeeded,
    Guid? ActionId,
    string? Error);

public interface IIrrigationClient
{
    Task<IrrigationRequestResult> RequestIrrigationAsync(
        IrrigationRequest request,
        CancellationToken cancellationToken = default);
}
