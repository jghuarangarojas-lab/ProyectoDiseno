using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsDelivery.Application.Abstractions;

namespace MsDelivery.Infrastructure.WhatsApp;

/// <summary>
/// Cliente de la WhatsApp Cloud API de Meta (graph.facebook.com).
/// Cuando <see cref="WhatsAppOptions.Mock"/> es true actúa como mock: solo escribe el
/// mensaje en el log y devuelve true, sin realizar llamadas de red.
/// </summary>
public sealed partial class MetaWhatsAppClient : IWhatsAppClient
{
    public const string HttpClientName = "whatsapp";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WhatsAppOptions _options;
    private readonly ILogger<MetaWhatsAppClient> _logger;

    public MetaWhatsAppClient(
        IHttpClientFactory httpClientFactory,
        IOptions<WhatsAppOptions> options,
        ILogger<MetaWhatsAppClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendTextAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
            return false;

        var to = NormalizePhoneNumber(phoneNumber);

        if (_options.Mock ||
            string.IsNullOrWhiteSpace(_options.AccessToken) ||
            string.IsNullOrWhiteSpace(_options.PhoneNumberId))
        {
            _logger.LogInformation("[MOCK WhatsApp] → {Phone}: {Message}", to, message);
            return true;
        }

        var payload = new
        {
            messaging_product = "whatsapp",
            to,
            type = "text",
            text = new { preview_url = false, body = message }
        };

        var url = $"{_options.ApiUrl.TrimEnd('/')}/{_options.ApiVersion}/{_options.PhoneNumberId}/messages";

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.AccessToken);

        var httpClient = _httpClientFactory.CreateClient(HttpClientName);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WhatsApp notificación enviada a {Phone}.", to);
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "WhatsApp API respondió {StatusCode} para {Phone}: {Body}",
                (int)response.StatusCode, to, errorBody);
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error de comunicación con WhatsApp API para {Phone}.", to);
            return false;
        }
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        var digits = DigitsRegex().Replace(phoneNumber, string.Empty);

        if (digits.Length is >= 8 and <= 15)
            return digits;

        if (!string.IsNullOrWhiteSpace(_options.DefaultCountryCode))
            return $"{_options.DefaultCountryCode}{digits}";

        return digits;
    }

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex DigitsRegex();
}
