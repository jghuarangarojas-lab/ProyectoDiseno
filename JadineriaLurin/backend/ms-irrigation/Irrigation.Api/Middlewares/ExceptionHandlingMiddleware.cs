using System.Text.Json;
using Irrigation.Application.Exceptions;
using Irrigation.Domain.Exceptions;

namespace Irrigation.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (IrrigationNotFoundException exception)
        {
            _logger.LogWarning(exception, "Recurso de riego no encontrado");
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, "NOT_FOUND", exception.Message);
        }
        catch (IrrigationDomainException exception)
        {
            _logger.LogWarning(exception, "Violación de regla de negocio en {Path}", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, "DOMAIN_ERROR", exception.Message);
        }
        catch (IrrigationApplicationException exception)
        {
            var statusCode = exception.InnerException is null
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status502BadGateway;

            _logger.LogError(exception, "Fallo de aplicación en {Path}", context.Request.Path);
            await WriteErrorAsync(context, statusCode, "APPLICATION_ERROR", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error no controlado en {Path}", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "Se produjo un error inesperado.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            statusCode,
            code,
            message,
            traceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(payload);
    }
}
