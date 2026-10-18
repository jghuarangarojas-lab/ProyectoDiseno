using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MsServices.Application.DTOs;
using MsServices.Application.Interfaces.Services;
using MsServices.Domain.Enums;

namespace MsServices.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Produces("application/json")]
public sealed class ProjectsController : ControllerBase
{
    private const long MaxAttachmentBytes = 20 * 1024 * 1024;

    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProjectSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? stage,
        [FromQuery] Guid? clientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ProjectStage? parsedStage = string.IsNullOrWhiteSpace(stage) ? null : stage.ToStage();
        var (items, totalCount) = await _projectService.ListAsync(parsedStage, clientId, page, pageSize, cancellationToken);

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        return Ok(items);
    }

    [HttpGet("{projectId:guid}")]
    [ProducesResponseType<ProjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(projectId, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    [ProducesResponseType<ProjectDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _projectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { projectId = project.ProjectId }, project);
    }

    [HttpPut("{projectId:guid}")]
    [ProducesResponseType<ProjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid projectId,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.UpdateAsync(projectId, request, cancellationToken);
        return Ok(project);
    }

    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid projectId, CancellationToken cancellationToken)
    {
        await _projectService.DeleteAsync(projectId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{projectId:guid}/stage")]
    [ProducesResponseType<ProjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStage(
        Guid projectId,
        [FromBody] ChangeStageRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.ChangeStageAsync(projectId, request.Stage, cancellationToken);
        return Ok(project);
    }

    [HttpGet("{projectId:guid}/attachments")]
    [ProducesResponseType<IReadOnlyList<AttachmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListAttachments(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(projectId, cancellationToken);
        return project is null ? NotFound() : Ok(project.Attachments);
    }

    [HttpPost("{projectId:guid}/attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(60 * 1024 * 1024)]
    [ProducesResponseType<IReadOnlyList<AttachmentDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAttachments(
        Guid projectId,
        [FromForm] AttachmentForm form,
        CancellationToken cancellationToken)
    {
        if (form.Files is null || form.Files.Count == 0)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Archivos requeridos",
                Detail = "Adjunte al menos un archivo en el campo 'Files' (multipart/form-data)."
            });

        var uploaded = new List<AttachmentDto>(form.Files.Count);

        foreach (var file in form.Files)
        {
            if (file.Length == 0)
                continue;

            if (file.Length > MaxAttachmentBytes)
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Archivo demasiado grande",
                    Detail = $"El archivo '{file.FileName}' supera el máximo de 20 MB."
                });

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            var attachment = await _projectService.AddAttachmentAsync(
                projectId,
                form.FileType,
                stream,
                file.FileName,
                file.ContentType,
                cancellationToken);

            uploaded.Add(attachment);
        }

        if (uploaded.Count == 0)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Archivos vacíos",
                Detail = "Ninguno de los archivos adjuntos tiene contenido."
            });

        return CreatedAtAction(nameof(ListAttachments), new { projectId }, uploaded);
    }

    [HttpDelete("{projectId:guid}/attachments/{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAttachment(
        Guid projectId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        await _projectService.RemoveAttachmentAsync(projectId, attachmentId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{projectId:guid}/installments")]
    [ProducesResponseType<IReadOnlyList<InstallmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListInstallments(Guid projectId, CancellationToken cancellationToken)
    {
        var installments = await _projectService.ListInstallmentsAsync(projectId, cancellationToken);
        return Ok(installments);
    }

    [HttpPost("{projectId:guid}/installments")]
    [ProducesResponseType<InstallmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddInstallment(
        Guid projectId,
        [FromBody] CreateInstallmentRequest request,
        CancellationToken cancellationToken)
    {
        var installment = await _projectService.AddInstallmentAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(ListInstallments), new { projectId }, installment);
    }

    [HttpPost("{projectId:guid}/installments/generate")]
    [ProducesResponseType<IReadOnlyList<InstallmentDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateInstallments(
        Guid projectId,
        [FromBody] GenerateInstallmentsRequest request,
        CancellationToken cancellationToken)
    {
        var installments = await _projectService.GenerateInstallmentsAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(ListInstallments), new { projectId }, installments);
    }

    [HttpPost("{projectId:guid}/installments/{installmentId:guid}/pay")]
    [ProducesResponseType<InstallmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkInstallmentPaid(
        Guid projectId,
        Guid installmentId,
        CancellationToken cancellationToken)
    {
        var installment = await _projectService.MarkInstallmentPaidAsync(projectId, installmentId, cancellationToken);
        return Ok(installment);
    }

    [HttpGet("{projectId:guid}/contract")]
    [ProducesResponseType<ContractDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContract(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return NotFound();

        return project.Contract is null ? NotFound() : Ok(project.Contract);
    }

    [HttpPost("{projectId:guid}/contract/generate")]
    [ProducesResponseType<ContractDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateContract(Guid projectId, CancellationToken cancellationToken)
    {
        var contract = await _projectService.GenerateContractAsync(projectId, cancellationToken);
        return CreatedAtAction(nameof(GetContract), new { projectId }, contract);
    }

    [HttpPost("{projectId:guid}/contract/sign")]
    [ProducesResponseType<ContractDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SignContract(
        Guid projectId,
        [FromBody] SignContractRequest request,
        CancellationToken cancellationToken)
    {
        var contract = await _projectService.SignContractAsync(projectId, request, cancellationToken);
        return Ok(contract);
    }

    [HttpPost("{projectId:guid}/billing/sync")]
    [ProducesResponseType<BillingSyncResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<BillingSyncResultDto>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncBilling(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _projectService.SyncBillingAsync(projectId, cancellationToken);
        return result.Accepted ? Ok(result) : StatusCode(StatusCodes.Status502BadGateway, result);
    }
}

public sealed class AttachmentForm
{
    [Required]
    [MaxLength(30)]
    public string FileType { get; set; } = null!;

    [Required]
    public List<IFormFile> Files { get; set; } = new();
}
