using Iot.Api.Hubs;
using Iot.Api.Middlewares;
using Iot.Api.Realtime;
using Iot.Application;
using Iot.Application.Interfaces.Realtime;
using Iot.Infrastructure;
using Iot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IIotStateNotifier, IotHubNotifier>();
builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ms-iot API",
        Version = "v1",
        Description = "Microservicio IoT: telemetría de sensores, umbrales botánicos, alertas de hardware, " +
                      "reubicación de sensores y actualizaciones en tiempo real por SignalR."
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");
app.MapControllers();
app.MapHub<IotHub>("/hubs/iot");

await ApplyDatabaseSchemaAsync(app);

app.Run();

static async Task ApplyDatabaseSchemaAsync(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var environment = application.Environment;
    var configuration = application.Configuration;

    var retryCount = configuration.GetValue<int?>("Database:MigrationRetryCount") ?? 3;
    var retryDelaySeconds = configuration.GetValue<int?>("Database:MigrationRetryDelaySeconds") ?? 5;
    var failFast = configuration.GetValue<bool?>("Database:MigrationFailFast") ?? !environment.IsDevelopment();

    for (var attempt = 1; attempt <= retryCount; attempt++)
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IotDbContext>();
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Esquema de la base de datos aplicado correctamente");
            return;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo aplicar el esquema de la base de datos (intento {Attempt}/{RetryCount})", attempt, retryCount);

            if (attempt == retryCount)
            {
                if (failFast)
                {
                    logger.LogCritical("Fallo al aplicar migraciones. Abortando arranque por política de resiliencia (MigrationFailFast=true).");
                    throw;
                }

                logger.LogWarning("Fallo al aplicar migraciones tras {RetryCount} intentos. Continuando el arranque en modo no controlado. Verifique la conexión a la base de datos.", retryCount);
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
        }
    }
}
