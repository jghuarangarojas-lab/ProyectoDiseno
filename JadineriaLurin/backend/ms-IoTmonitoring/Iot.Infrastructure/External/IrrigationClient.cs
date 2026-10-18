using System.Net.Http.Json;
using System.Text.Json;
using Iot.Application.Interfaces.External;
using Microsoft.Extensions.Logging;

namespace Iot.Infrastructure.External;

public sealed class IrrigationClient : IIrrigationClient
{
    private const string StartPath = "api/irrigation/start";
    private const int MaxErrorLength = 300;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<IrrigationClient> _logger;

    public IrrigationClient(HttpClient httpClient, ILogger<IrrigationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IrrigationRequestResult> RequestIrrigationAsync(
        IrrigationRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            deviceId = request.DeviceId,
            sectorArea = request.SectorArea,
            triggerType = "AUTOMATIC",
            requestedDurationSeconds = request.DurationSeconds
        };

        using var response = await _httpClient.PostAsJsonAsync(StartPath, payload, SerializerOptions, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var actionId = TryExtractActionId(body);
            _logger.LogInformation(
                "ms-irrigation aceptó la solicitud de riego para '{DeviceId}' en '{Sector}' (acción {ActionId})",
                request.DeviceId,
                request.SectorArea,
                actionId);
            return new IrrigationRequestResult(true, actionId, null);
        }

        var error = Truncate($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        _logger.LogWarning(
            "ms-irrigation devolvió un error para '{DeviceId}': {Error}",
            request.DeviceId,
            error);
        return new IrrigationRequestResult(false, null, error);
    }

    private static Guid? TryExtractActionId(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (string.Equals(property.Name, "id", StringComparison.OrdinalIgnoreCase) &&
                    property.Value.ValueKind == JsonValueKind.String &&
                    Guid.TryParse(property.Value.GetString(), out var actionId))
                {
                    return actionId;
                }
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string Truncate(string value)
    {
        if (value.Length <= MaxErrorLength)
        {
            return value;
        }

        return string.Concat(value.AsSpan(0, MaxErrorLength), "...");
    }
}
