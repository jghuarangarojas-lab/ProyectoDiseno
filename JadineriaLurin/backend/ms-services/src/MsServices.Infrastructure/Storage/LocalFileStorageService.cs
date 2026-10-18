using Microsoft.Extensions.Options;
using MsServices.Application.Abstractions;
using MsServices.Domain.Exceptions;

namespace MsServices.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf"
    };

    private readonly LocalFileStorageOptions _options;

    public LocalFileStorageService(IOptions<LocalFileStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new DomainException(
                $"Tipo de archivo no permitido: '{extension}'. Extensiones válidas: {string.Join(", ", AllowedExtensions)}.");

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var dateSegment = DateTime.UtcNow.ToString("yyyy/MM");
        var relativeFolder = $"{folder.Trim('/')}/{dateSegment}";
        var fullFolder = Path.Combine(_options.RootPath, relativeFolder.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(fullFolder);

        var fullPath = Path.Combine(fullFolder, storedFileName);
        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"{_options.BaseUrl.TrimEnd('/')}/{relativeFolder}/{storedFileName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return Task.CompletedTask;

        var baseUrl = "/" + _options.BaseUrl.Trim('/');
        if (!fileUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var relativePath = fileUrl[baseUrl.Length..].TrimStart('/');
        var fullPath = Path.GetFullPath(Path.Combine(
            _options.RootPath,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));

        var rootPath = Path.GetFullPath(_options.RootPath);
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
