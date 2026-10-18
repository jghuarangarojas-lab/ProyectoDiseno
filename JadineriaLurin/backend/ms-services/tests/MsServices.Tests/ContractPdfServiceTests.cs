using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;
using MsServices.Infrastructure.Pdf;
using MsServices.Infrastructure.Storage;
using Xunit;

namespace MsServices.Tests;

public class ContractPdfServiceTests
{
    private static ContractPdfService CreateService(string templatePath = "") => new(
        Options.Create(new ContractPdfOptions { TemplatePath = templatePath }),
        Options.Create(new LocalFileStorageOptions()),
        NullLogger<ContractPdfService>.Instance);

    private static ServiceProject CreateSampleProject()
    {
        var now = DateTime.UtcNow;
        var project = new ServiceProject
        {
            ProjectId = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            Title = "Diseño de jardín frontal",
            SiteAddress = "Mz. C Lote 12, Lurín",
            Stage = ProjectStage.APPROVED,
            TotalBudget = 3000m,
            CreatedAt = now,
            UpdatedAt = now
        };

        project.AddInstallment(1000m, now.AddDays(30), now);
        project.AddInstallment(1000m, now.AddDays(60), now);
        project.AddInstallment(1000m, now.AddDays(90), now);

        return project;
    }

    private static string ExtractText(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes);
        using var document = new PdfDocument(new PdfReader(stream));
        var text = string.Empty;
        for (var page = 1; page <= document.GetNumberOfPages(); page++)
            text += PdfTextExtractor.GetTextFromPage(document.GetPage(page));
        return text;
    }

    [Fact]
    public async Task GenerateAsync_ReturnsPdfWithProjectAndInstallmentData()
    {
        var project = CreateSampleProject();

        var pdfBytes = await CreateService()
            .GenerateAsync(project, project.Installments);

        Assert.True(pdfBytes.Length > 1000, $"PDF demasiado pequeño: {pdfBytes.Length} bytes");
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4));

        var text = ExtractText(pdfBytes);
        Assert.Contains(project.Title, text);
        Assert.Contains(project.ClientId.ToString(), text);
        Assert.Contains("S/ 1,000.00", text);
        Assert.Contains("cuotas", text);
        Assert.Contains("APPROVED", text);
    }

    [Fact]
    public async Task GenerateAsync_UsesExternalTemplateWhenConfigured()
    {
        var project = CreateSampleProject();
        var templatePath = Path.Combine(Path.GetTempPath(), $"plantilla-contrato-{Guid.NewGuid():N}.html");

        try
        {
            File.WriteAllText(templatePath,
                "<html><body><h1>PLANTILLA EXTERNA PERSONALIZADA</h1><p>{{Title}} - {{TotalBudget}}</p></body></html>");

            var pdfBytes = await CreateService(templatePath)
                .GenerateAsync(project, project.Installments);

            var text = ExtractText(pdfBytes);
            Assert.Contains("PLANTILLA EXTERNA PERSONALIZADA", text);
            Assert.Contains(project.Title, text);
        }
        finally
        {
            if (File.Exists(templatePath))
                File.Delete(templatePath);
        }
    }

    [Fact]
    public async Task GenerateAsync_WithoutInstallments_KeepsTemplateValid()
    {
        var project = CreateSampleProject();
        project.Installments.Clear();

        var pdfBytes = await CreateService()
            .GenerateAsync(project, project.Installments);

        var text = ExtractText(pdfBytes);
        Assert.Contains("Sin cuotas registradas", text);
    }
}
