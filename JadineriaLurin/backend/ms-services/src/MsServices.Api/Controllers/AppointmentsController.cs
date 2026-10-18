using Microsoft.AspNetCore.Mvc;
using MsServices.Application.DTOs;
using MsServices.Application.Interfaces.Services;

namespace MsServices.Api.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("projects/{projectId:guid}/appointments")]
    [ProducesResponseType<IReadOnlyList<AppointmentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.ListByProjectAsync(projectId, cancellationToken);
        return Ok(appointments);
    }

    [HttpPost("projects/{projectId:guid}/appointments")]
    [ProducesResponseType<AppointmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Schedule(
        Guid projectId,
        [FromBody] ScheduleAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.ScheduleAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { appointmentId = appointment.AppointmentId }, appointment);
    }

    [HttpGet("appointments/{appointmentId:guid}")]
    [ProducesResponseType<AppointmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid appointmentId, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetByIdAsync(appointmentId, cancellationToken);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpPost("appointments/{appointmentId:guid}/reschedule")]
    [ProducesResponseType<RescheduleAppointmentResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reschedule(
        Guid appointmentId,
        [FromBody] RescheduleAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.RescheduleAsync(appointmentId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("appointments/{appointmentId:guid}/cancel")]
    [ProducesResponseType<AppointmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        Guid appointmentId,
        [FromBody] CancelAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.CancelAsync(appointmentId, request, cancellationToken);
        return Ok(appointment);
    }
}
