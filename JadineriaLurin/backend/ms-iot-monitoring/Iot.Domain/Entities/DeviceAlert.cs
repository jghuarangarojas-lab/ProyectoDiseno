using Iot.Domain.Enums;
using Iot.Domain.Exceptions;

namespace Iot.Domain.Entities;

public class DeviceAlert
{
    public const int MaxMessageLength = 512;
    public const int MaxResolutionNoteLength = 512;

    public Guid Id { get; private set; }
    public string DeviceCode { get; private set; } = string.Empty;
    public AlertIssueType IssueType { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsResolved { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public string? ResolutionNote { get; private set; }

    private DeviceAlert()
    {
    }

    public static DeviceAlert Create(
        string deviceCode,
        AlertIssueType issueType,
        string message,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new IotDomainException("El código del dispositivo (DeviceCode) es obligatorio.");
        }

        if (!Enum.IsDefined(issueType))
        {
            throw new IotDomainException("El tipo de incidencia (IssueType) no es válido.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new IotDomainException("El mensaje de la alerta es obligatorio.");
        }

        return new DeviceAlert
        {
            Id = Guid.NewGuid(),
            DeviceCode = deviceCode.Trim(),
            IssueType = issueType,
            Message = message.Trim().Length > MaxMessageLength
                ? message.Trim()[..MaxMessageLength]
                : message.Trim(),
            IsResolved = false,
            CreatedAtUtc = utcNow,
            ResolvedAtUtc = null
        };
    }

    public void Resolve(string? resolutionNote, DateTime utcNow)
    {
        if (IsResolved)
        {
            throw new IotDomainException(
                $"La alerta {Id} del dispositivo {DeviceCode} ya fue resuelta el {ResolvedAtUtc:u}.");
        }

        if (resolutionNote is not null && resolutionNote.Length > MaxResolutionNoteLength)
        {
            throw new IotDomainException(
                $"La nota de resolución no puede superar los {MaxResolutionNoteLength} caracteres.");
        }

        IsResolved = true;
        ResolvedAtUtc = utcNow;
        ResolutionNote = string.IsNullOrWhiteSpace(resolutionNote) ? null : resolutionNote.Trim();
    }
}
