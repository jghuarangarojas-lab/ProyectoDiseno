using Irrigation.Api.Hubs;
using Irrigation.Api.Middlewares;
using Irrigation.Api.Realtime;
using Irrigation.Application;
using Irrigation.Application.Interfaces.Realtime;
using Irrigation.Infrastructure;
using Irrigation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IIrrigationStateNotifier, IrrigationHubNotifier>();
builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ms-irrigation API",
        Version = "v1",
        Description = "Microservicio de riego: comandos MQTT, ciclo de vida de acciones de riego y actualizaciones en tiempo real por SignalR."
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
app.MapHub<IrrigationHub>("/hubs/irrigation");

await ApplyDatabaseSchemaAsync(app);

app.Run();

static async Task ApplyDatabaseSchemaAsync(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IrrigationDbContext>();
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Esquema de la base de datos aplicado correctamente");
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "No se pudo aplicar el esquema de la base de datos");
    }
}
