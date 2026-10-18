using System.Text.Json.Serialization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using MsServices.Api.Middleware;
using MsServices.Application;
using MsServices.Domain.Enums;
using MsServices.Infrastructure;
using MsServices.Infrastructure.Persistence;
using MsServices.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

app.UseMiddleware<DomainExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ServicesDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseCors("Frontend");

var fileStorageOptions = app.Configuration
        .GetSection(LocalFileStorageOptions.SectionName)
        .Get<LocalFileStorageOptions>()
    ?? new LocalFileStorageOptions();

var storageRootPath = Path.GetFullPath(fileStorageOptions.RootPath);
Directory.CreateDirectory(storageRootPath);

var filesRequestPath = string.IsNullOrWhiteSpace(fileStorageOptions.BaseUrl)
    ? "/files"
    : $"/{fileStorageOptions.BaseUrl.Trim('/')}";

var contentTypeProvider = new FileExtensionContentTypeProvider();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storageRootPath),
    RequestPath = filesRequestPath,
    ContentTypeProvider = contentTypeProvider
});

app.MapControllers();

app.Run();
