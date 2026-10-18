using Microsoft.Extensions.Options;
using MsInventory.Application.Abstractions;

namespace MsInventory.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
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
            throw new InvalidOperationException($"Tipo de archivo no permitido: '{extension}'. Extensiones válidas: {string.Join(", ", AllowedExtensions)}.");

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var dateSegment = DateTime.UtcNow.ToString("yyyy/MM");
        var relativeFolder = $"{folder}/{dateSegment}";
        var fullFolder = Path.Combine(_options.RootPath, relativeFolder.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(fullFolder);

        var fullPath = Path.Combine(fullFolder, storedFileName);
        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"{_options.BaseUrl.TrimEnd('/')}/{relativeFolder}/{storedFileName}";
    }
}
