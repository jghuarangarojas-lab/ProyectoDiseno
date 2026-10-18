using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsServices.Application.DTOs;
using MsServices.Application.Interfaces.External;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;

namespace MsServices.Infrastructure.Billing;

/// <summary>
/// Envía el arreglo de PaymentInstallment a ms-commercial (caja) por HTTP
/// para que sepa cuándo y cuánto cobrar.
/// </summary>
public sealed class BillingIntegration : IBillingIntegration
{
    public const string HttpClientName = "ms-commercial";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MsCommercialOptions _options;
    private readonly ILogger<BillingIntegration> _logger;

    public BillingIntegration(
        IHttpClientFactory httpClientFactory,
        IOptions<MsCommercialOptions> options,
        ILogger<BillingIntegration> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<BillingSyncResultDto> SendInstallmentsAsync(
        ServiceProject project,
        IReadOnlyList<PaymentInstallment> installments,
        CancellationToken cancellationToken = default)
    {
        var payload = new BillingPayload(
            project.ProjectId,
            project.ClientId,
            project.Title,
            project.Stage.ToCode(),
            project.TotalBudget,
            DateTime.UtcNow,
            installments.Select(i => new BillingInstallmentPayload(
                i.InstallmentId,
                i.Amount,
                i.DueDate,
                i.Status.ToCode(),
                i.PaidAt)).ToList());

        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        var sentAt = DateTime.UtcNow;

        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(_options.InstallmentsPath, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "ms-commercial rechazó el plan de cobros del proyecto {ProjectId}: {StatusCode} {Body}",
                    project.ProjectId, (int)response.StatusCode, Truncate(responseBody));

                return new BillingSyncResultDto
                {
                    Accepted = false,
                    ProjectId = project.ProjectId,
                    InstallmentCount = installments.Count,
                    TotalAmount = installments.Sum(i => i.Amount),
                    HttpStatus = (int)response.StatusCode,
                    SentAt = sentAt,
                    Message = $"ms-commercial respondió {(int)response.StatusCode}: {Truncate(responseBody)}"
                };
            }

            _logger.LogInformation(
                "Plan de cobros del proyecto {ProjectId} enviado a ms-commercial: {Count} cuotas por {Total}.",
                project.ProjectId, installments.Count, installments.Sum(i => i.Amount));

            return new BillingSyncResultDto
            {
                Accepted = true,
                ProjectId = project.ProjectId,
                InstallmentCount = installments.Count,
                TotalAmount = installments.Sum(i => i.Amount),
                HttpStatus = (int)response.StatusCode,
                SentAt = sentAt,
                Message = "Plan de cobros recibido por caja."
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Tiempo de espera agotado al enviar el plan de cobros del proyecto {ProjectId} a ms-commercial.",
                project.ProjectId);

            return Failure(project, installments, sentAt,
                $"ms-commercial no respondió en {_options.TimeoutSeconds} segundos.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "No fue posible contactar a ms-commercial para el proyecto {ProjectId}.",
                project.ProjectId);

            return Failure(project, installments, sentAt,
                $"No fue posible contactar a ms-commercial: {exception.Message}");
        }
    }

    private static BillingSyncResultDto Failure(
        ServiceProject project,
        IReadOnlyList<PaymentInstallment> installments,
        DateTime sentAt,
        string message) => new()
    {
        Accepted = false,
        ProjectId = project.ProjectId,
        InstallmentCount = installments.Count,
        TotalAmount = installments.Sum(i => i.Amount),
        SentAt = sentAt,
        Message = message
    };

    private static string Truncate(string value) =>
        value.Length <= 300 ? value : value[..300] + "…";
}
