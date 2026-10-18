using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MsServices.Application.DTOs;
using MsServices.Domain.Entities;
using MsServices.Domain.Enums;
using MsServices.Domain.Exceptions;
using MsServices.Application.Services;
using MsServices.Infrastructure.Billing;
using MsServices.Infrastructure.Persistence;
using MsServices.Infrastructure.Persistence.Repositories;
using MsServices.Infrastructure.Pdf;
using MsServices.Infrastructure.Storage;
using Xunit;

namespace MsServices.Tests;

public class ProjectLifecycleIntegrationTests : IDisposable
{
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), $"ms-services-test-{Guid.NewGuid():N}");
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ServicesDbContext> _dbContextOptions;
    private readonly ServicesDbContext _dbContext;

    public ProjectLifecycleIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _dbContextOptions = new DbContextOptionsBuilder<ServicesDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ServicesDbContext(_dbContextOptions);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();

        if (Directory.Exists(_storageRoot))
            Directory.Delete(_storageRoot, recursive: true);
    }

    private ProjectService CreateProjectService(StubCommercialHandler? commercialHandler = null)
    {
        var fileStorage = new LocalFileStorageService(Options.Create(new LocalFileStorageOptions
        {
            RootPath = _storageRoot,
            BaseUrl = "/files"
        }));

        var pdfService = new ContractPdfService(
            Options.Create(new ContractPdfOptions()),
            Options.Create(new LocalFileStorageOptions { RootPath = _storageRoot, BaseUrl = "/files" }),
            NullLogger<ContractPdfService>.Instance);

        var billing = new BillingIntegration(
            new StubHttpClientFactory(commercialHandler ?? new StubCommercialHandler()),
            Options.Create(new MsCommercialOptions()),
            NullLogger<BillingIntegration>.Instance);

        return new ProjectService(
            new ServiceProjectRepository(_dbContext),
            fileStorage,
            pdfService,
            billing);
    }

    private AppointmentService CreateAppointmentService() => new(
        new AppointmentRepository(_dbContext),
        new ServiceProjectRepository(_dbContext));

    [Fact]
    public async Task FullLifecycle_CreateSignContractAndRescheduleAppointment()
    {
        var projectService = CreateProjectService();
        var appointmentService = CreateAppointmentService();

        var created = await projectService.CreateAsync(new CreateProjectRequest
        {
            ClientId = Guid.NewGuid(),
            Title = "Diseño de jardín con estanque",
            TotalBudget = 4500m,
            SiteAddress = "Lurín"
        });
        Assert.Equal("REQUESTED", created.Stage);

        var signatureBase64 = Convert.ToBase64String(CreateTinyPng());
        var signatureRequest = new SignContractRequest
        {
            SignatureImageBase64 = $"data:image/png;base64,{signatureBase64}",
            SignedBy = "cliente@correo.com"
        };

        var unsignedException = await Assert.ThrowsAsync<DomainException>(
            () => projectService.SignContractAsync(created.ProjectId, signatureRequest));
        Assert.Contains("APPROVED", unsignedException.Message);

        foreach (var stage in new[] { "TECH_VISIT", "PROPOSAL", "APPROVED" })
        {
            var updated = await projectService.ChangeStageAsync(created.ProjectId, stage);
            Assert.Equal(stage, updated.Stage);
        }

        var installments = await projectService.GenerateInstallmentsAsync(created.ProjectId,
            new GenerateInstallmentsRequest
            {
                Count = 3,
                StartDate = DateTime.UtcNow.AddDays(30),
                IntervalDays = 30
            });
        Assert.Equal(3, installments.Count);
        Assert.Equal(4500m, installments.Sum(i => i.Amount));

        var invoiceBytes = Encoding.UTF8.GetBytes("FACTURA-001");
        var attachment = await projectService.AddAttachmentAsync(
            created.ProjectId,
            "INVOICE",
            new MemoryStream(invoiceBytes),
            "factura-001.pdf",
            "application/pdf");
        Assert.Equal("INVOICE", attachment.FileType);
        Assert.StartsWith("/files/projects/attachments/", attachment.FileUrl);
        Assert.True(File.Exists(UrlToFullPath(attachment.FileUrl)));

        var exception = await Assert.ThrowsAsync<DomainException>(() => projectService.AddAttachmentAsync(
            created.ProjectId,
            "INVOICE",
            new MemoryStream(Encoding.UTF8.GetBytes("no es pdf")),
            "factura.docx",
            "application/msword"));
        Assert.Contains("PDF", exception.Message);

        var contract = await projectService.GenerateContractAsync(created.ProjectId);
        Assert.NotNull(contract.GeneratedPdfUrl);
        Assert.True(File.Exists(UrlToFullPath(contract.GeneratedPdfUrl!)));

        var signed = await projectService.SignContractAsync(created.ProjectId, signatureRequest);
        Assert.NotNull(signed.SignedAt);
        Assert.NotNull(signed.DigitalSignatureUrl);
        Assert.True(File.Exists(UrlToFullPath(signed.DigitalSignatureUrl!)));

        var afterSigning = await projectService.GetByIdAsync(created.ProjectId);
        Assert.Equal("CONTRACTED", afterSigning!.Stage);

        var billed = await projectService.SyncBillingAsync(created.ProjectId);
        Assert.True(billed.Accepted, billed.Message);
        Assert.Equal(3, billed.InstallmentCount);
        Assert.Equal(4500m, billed.TotalAmount);

        var scheduled = await appointmentService.ScheduleAsync(created.ProjectId, new ScheduleAppointmentRequest
        {
            GardenerId = Guid.NewGuid(),
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            Notes = "Visita técnica"
        });
        Assert.Equal("SCHEDULED", scheduled.Status);

        var reschedule = await appointmentService.RescheduleAsync(
            scheduled.AppointmentId,
            new RescheduleAppointmentRequest
            {
                NewScheduledDate = DateTime.UtcNow.AddDays(11),
                Notes = "Cliente cambió de fecha"
            });

        Assert.Equal("RESCHEDULED", reschedule.Original.Status);
        Assert.Equal("SCHEDULED", reschedule.Replacement.Status);
        Assert.Equal(scheduled.AppointmentId, reschedule.Replacement.RescheduledFromId);
        Assert.Equal(scheduled.ScheduledDate, reschedule.Original.ScheduledDate);

        var appointments = await appointmentService.ListByProjectAsync(created.ProjectId);
        Assert.Equal(2, appointments.Count);

        await Assert.ThrowsAsync<DomainException>(() => appointmentService.RescheduleAsync(
            scheduled.AppointmentId,
            new RescheduleAppointmentRequest { NewScheduledDate = DateTime.UtcNow.AddDays(20) }));
    }

    [Fact]
    public async Task SyncBilling_SendsInstallmentsArrayToCommercial_AndReportsFailure()
    {
        var handler = new StubCommercialHandler();
        var projectService = CreateProjectService(handler);

        var project = await projectService.CreateAsync(new CreateProjectRequest
        {
            ClientId = Guid.NewGuid(),
            Title = "Mantenimiento mensual",
            TotalBudget = 600m
        });

        await projectService.GenerateInstallmentsAsync(project.ProjectId, new GenerateInstallmentsRequest
        {
            Count = 2,
            StartDate = DateTime.UtcNow.AddDays(15),
            IntervalDays = 15
        });

        var result = await projectService.SyncBillingAsync(project.ProjectId);

        Assert.True(result.Accepted);
        Assert.NotNull(handler.LastBody);

        using var payload = JsonDocument.Parse(handler.LastBody!);
        var root = payload.RootElement;
        Assert.Equal(project.ProjectId, root.GetProperty("projectId").GetGuid());
        Assert.Equal(2, root.GetProperty("installments").GetArrayLength());
        Assert.Equal("PENDING", root.GetProperty("installments")[0].GetProperty("status").GetString());

        handler.StatusCode = HttpStatusCode.InternalServerError;
        var failed = await projectService.SyncBillingAsync(project.ProjectId);
        Assert.False(failed.Accepted);
        Assert.Equal(500, failed.HttpStatus);
    }

    private string UrlToFullPath(string fileUrl)
    {
        var relative = fileUrl["/files".Length..].TrimStart('/');
        return Path.GetFullPath(Path.Combine(_storageRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static byte[] CreateTinyPng()
    {
        // PNG mínimo de 1x1 px válido.
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public StubHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) =>
            new(_handler, disposeHandler: false) { BaseAddress = new Uri("http://ms-commercial.test/") };
    }

    private sealed class StubCommercialHandler : HttpMessageHandler
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string? LastBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(StatusCode)
            {
                Content = new StringContent("{\"ok\":true}", Encoding.UTF8, "application/json")
            };
        }
    }
}
