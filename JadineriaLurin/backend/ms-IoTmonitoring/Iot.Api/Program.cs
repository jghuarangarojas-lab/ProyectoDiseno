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

    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IotDbContext>();
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Esquema de la base de datos aplicado correctamente");
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "No se pudo aplicar el esquema de la base de datos");
    }
}
