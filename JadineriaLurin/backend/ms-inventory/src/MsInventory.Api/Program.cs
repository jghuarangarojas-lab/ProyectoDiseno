using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using MsInventory.Api.Middleware;
using MsInventory.Application;
using MsInventory.Infrastructure;
using MsInventory.Infrastructure.Hubs;
using MsInventory.Infrastructure.Persistence;
using MsInventory.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };

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
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    var created = await dbContext.Database.EnsureCreatedAsync();
    if (created)
        await InventoryDbContextSeed.SeedAsync(dbContext);
}

app.UseCors("Frontend");

var fileStorageOptions = app.Configuration
        .GetSection(LocalFileStorageOptions.SectionName)
        .Get<LocalFileStorageOptions>()
    ?? new LocalFileStorageOptions();

var storageRootPath = Path.GetFullPath(fileStorageOptions.RootPath);
Directory.CreateDirectory(storageRootPath);

var evidenceRequestPath = string.IsNullOrWhiteSpace(fileStorageOptions.BaseUrl)
    ? "/evidence"
    : $"/{fileStorageOptions.BaseUrl.Trim('/')}";

var contentTypeProvider = new FileExtensionContentTypeProvider();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storageRootPath),
    RequestPath = evidenceRequestPath,
    ContentTypeProvider = contentTypeProvider
});

app.MapControllers();
app.MapHub<InventoryHub>("/hubs/inventory");

app.Run();
