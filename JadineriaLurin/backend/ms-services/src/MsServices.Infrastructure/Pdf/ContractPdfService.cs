using System.Reflection;
using System.Text;
using iText.Html2pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsServices.Application.Abstractions;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;
using MsServices.Infrastructure.Storage;

namespace MsServices.Infrastructure.Pdf;

/// <summary>
/// Toma una plantilla HTML/texto (embebida o configurada en ContractPdf:TemplatePath),
/// la rellena con los datos del ServiceProject y su plan de PaymentInstallment
/// y la convierte a PDF con iText7 (pdfHTML).
/// </summary>
public sealed class ContractPdfService : IContractPdfService
{
    private const string EmbeddedTemplateName = "MsServices.Infrastructure.Pdf.Templates.contract-template.html";

    private readonly ContractPdfOptions _options;
    private readonly LocalFileStorageOptions _storageOptions;
    private readonly ILogger<ContractPdfService> _logger;

    public ContractPdfService(
        IOptions<ContractPdfOptions> options,
        IOptions<LocalFileStorageOptions> storageOptions,
        ILogger<ContractPdfService> logger)
    {
        _options = options.Value;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    public Task<byte[]> GenerateAsync(
        ServiceProject project,
        IReadOnlyList<PaymentInstallment> installments,
        CancellationToken cancellationToken = default)
    {
        var template = LoadTemplate();
        var html = FillTemplate(template, project, installments);

        var output = new MemoryStream();
        var properties = new ConverterProperties();
        properties.SetBaseUri(AppContext.BaseDirectory);

        HtmlConverter.ConvertToPdf(html, output, properties);

        // iText cierra el stream al finalizar; MemoryStream.ToArray() sigue siendo válido.
        var pdfBytes = output.ToArray();

        _logger.LogInformation(
            "Contrato PDF generado para el proyecto {ProjectId} ({ByteCount} bytes, {InstallmentCount} cuotas).",
            project.ProjectId, pdfBytes.Length, installments.Count);

        return Task.FromResult(pdfBytes);
    }

    private string LoadTemplate()
    {
        if (!string.IsNullOrWhiteSpace(_options.TemplatePath) && File.Exists(_options.TemplatePath))
            return File.ReadAllText(_options.TemplatePath, Encoding.UTF8);

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(EmbeddedTemplateName)
            ?? throw new InvalidOperationException(
                $"No se encontró la plantilla embebida '{EmbeddedTemplateName}'. Recursos: {string.Join(", ", assembly.GetManifestResourceNames())}.");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private string FillTemplate(string template, ServiceProject project, IReadOnlyList<PaymentInstallment> installments)
    {
        var now = DateTime.UtcNow;
        var rows = new StringBuilder();

        var ordered = installments.OrderBy(i => i.DueDate).ToList();
        for (var index = 0; index < ordered.Count; index++)
        {
            var installment = ordered[index];
            rows.AppendLine(
                "<tr>" +
                $"<td>{index + 1}</td>" +
                $"<td>{installment.DueDate:dd/MM/yyyy}</td>" +
                $"<td class=\"amount\">{FormatMoney(installment.Amount)}</td>" +
                $"<td>{installment.Status.ToCode()}</td>" +
                "</tr>");
        }

        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ContractNumber"] = $"CT-{project.ProjectId.ToString()[..8].ToUpperInvariant()}",
            ["ProjectId"] = project.ProjectId.ToString(),
            ["ClientId"] = project.ClientId.ToString(),
            ["Title"] = Html(project.Title),
            ["SiteAddress"] = Html(project.SiteAddress ?? "Por definir"),
            ["Stage"] = project.Stage.ToCode(),
            ["TotalBudget"] = FormatMoney(project.TotalBudget),
            ["GeneratedAt"] = now.ToString("dd/MM/yyyy HH:mm") + " UTC",
            ["InstallmentsCount"] = ordered.Count.ToString(),
            ["InstallmentsTable"] = rows.Length == 0
                ? "<tr><td colspan=\"4\">Sin cuotas registradas</td></tr>"
                : rows.ToString(),
            ["CompanyName"] = Html(_options.CompanyName),
            ["CompanyRuc"] = Html(_options.CompanyRuc),
            ["CompanyAddress"] = Html(_options.CompanyAddress),
            ["ContactEmail"] = Html(_options.ContactEmail),
            ["SignatureImage"] = BuildSignatureImage(project.Contract)
        };

        var filled = template;
        foreach (var (token, value) in tokens)
            filled = filled.Replace($"{{{{{token}}}}}", value, StringComparison.Ordinal);

        return filled;
    }

    private string BuildSignatureImage(ContractDocument? contract)
    {
        if (contract is null || !contract.IsSigned)
            return string.Empty;

        try
        {
            var baseUrl = "/" + _storageOptions.BaseUrl.Trim('/');
            var fileUrl = contract.DigitalSignatureUrl!;

            if (!fileUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            var relativePath = fileUrl[baseUrl.Length..].TrimStart('/');
            var fullPath = Path.GetFullPath(Path.Combine(
                _storageOptions.RootPath,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));

            if (!File.Exists(fullPath))
                return string.Empty;

            var bytes = File.ReadAllBytes(fullPath);
            var mediaType = Path.GetExtension(fullPath).Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                ? "image/jpeg"
                : "image/png";

            return $"<img class=\"signature-image\" src=\"data:{mediaType};base64,{Convert.ToBase64String(bytes)}\" alt=\"Firma digital\" />";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "No fue posible incrustar la firma en el contrato.");
            return string.Empty;
        }
    }

    private static string FormatMoney(decimal value) =>
        $"S/ {value.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)}";

    private static string Html(string? value) => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
}
