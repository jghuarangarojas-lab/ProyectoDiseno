using Iot.Domain.Entities;

namespace Iot.Application.DTOs.Responses;

public sealed record DeviceAlertDto
{
    public Guid Id { get; init; }
    public string DeviceCode { get; init; } = string.Empty;
    public string IssueType { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsResolved { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? ResolvedAtUtc { get; init; }
    public string? ResolutionNote { get; init; }

    public static DeviceAlertDto From(DeviceAlert alert)
    {
        return new DeviceAlertDto
        {
            Id = alert.Id,
            DeviceCode = alert.DeviceCode,
            IssueType = alert.IssueType.ToString(),
            Message = alert.Message,
            IsResolved = alert.IsResolved,
            CreatedAtUtc = alert.CreatedAtUtc,
            ResolvedAtUtc = alert.ResolvedAtUtc,
            ResolutionNote = alert.ResolutionNote
        };
    }
}
