using Microsoft.Extensions.Options;
using MsInventory.Application.Abstractions;

namespace MsInventory.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly LocalFileStorageOptions _options;

    public LocalFileStorageService(IOptions<LocalFileStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Nombre de archivo inválido.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(folder))
            throw new ArgumentException("Carpeta inválida.", nameof(folder));

        if (content.CanSeek && content.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"El archivo excede el tamaño máximo permitido ({MaxFileSizeBytes / 1024 / 1024} MB).");

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            throw new InvalidOperationException("El archivo debe tener una extensión.");
        extension = extension.ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"Tipo de archivo no permitido: '{extension}'. Extensiones válidas: {string.Join(", ", AllowedExtensions)}.");

        var sanitizedFolder = folder.Replace('\\', '/').Trim().Trim('/');
        if (string.IsNullOrWhiteSpace(sanitizedFolder))
            throw new ArgumentException("Carpeta inválida.", nameof(folder));
        if (sanitizedFolder.Contains("..") || sanitizedFolder.Contains(':'))
            throw new InvalidOperationException("Ruta de carpeta no permitida.");

        var rootFullPath = Path.GetFullPath(_options.RootPath);
        Directory.CreateDirectory(rootFullPath);

        var dateSegment = DateTime.UtcNow.ToString("yyyy/MM");
        var relativeFolder = $"{sanitizedFolder}/{dateSegment}";
        var relativeFolderNormalized = relativeFolder.Replace('/', Path.DirectorySeparatorChar);
        var fullFolder = Path.GetFullPath(Path.Combine(rootFullPath, relativeFolderNormalized));

        if (!fullFolder.StartsWith(rootFullPath + Path.DirectorySeparatorChar) && !string.Equals(fullFolder, rootFullPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Ruta fuera del directorio permitido.");

        Directory.CreateDirectory(fullFolder);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPathResolved = Path.GetFullPath(Path.Combine(fullFolder, storedFileName));

        if (!fullPathResolved.StartsWith(rootFullPath + Path.DirectorySeparatorChar))
            throw new InvalidOperationException("Ruta fuera del directorio permitido.");

        await using var fileStream = File.Create(fullPathResolved);
        await content.CopyToAsync(fileStream, cancellationToken);

        if (fileStream.Length > MaxFileSizeBytes)
        {
            try { fileStream.Close(); } catch { }
            try { File.Delete(fullPathResolved); } catch { }
            throw new InvalidOperationException($"El archivo excede el tamaño máximo permitido ({MaxFileSizeBytes / 1024 / 1024} MB).");
        }

        return $"{_options.BaseUrl.TrimEnd('/')}/{relativeFolder}/{storedFileName}";
    }
}
