namespace Iot.Application.DTOs.Requests;

public sealed record MarkUnavailableDto
{
    public string? Reason { get; init; }
}
