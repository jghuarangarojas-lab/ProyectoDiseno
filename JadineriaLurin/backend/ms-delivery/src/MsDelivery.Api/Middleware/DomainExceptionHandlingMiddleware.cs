using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MsDelivery.Domain.Exceptions;

namespace MsDelivery.Api.Middleware;

public sealed class DomainExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionHandlingMiddleware> _logger;

    public DomainExceptionHandlingMiddleware(RequestDelegate next, ILogger<DomainExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            DeliveryOrderNotFoundException => (StatusCodes.Status404NotFound, "Orden de entrega no encontrada"),
            DriverNotFoundException => (StatusCodes.Status404NotFound, "Repartidor no encontrado"),
            InvalidStatusTransitionException => (StatusCodes.Status409Conflict, "Transición de estado no permitida"),
            ProofOfDeliveryMissingException => (StatusCodes.Status400BadRequest, "Prueba de entrega requerida"),
            DomainException => (StatusCodes.Status400BadRequest, "Error de negocio"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Excepción no controlada procesando {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogInformation(exception, "Rechazada solicitud en {Path} con {StatusCode}", context.Request.Path, statusCode);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var payload = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
