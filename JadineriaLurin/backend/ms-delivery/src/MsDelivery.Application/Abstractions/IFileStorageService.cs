namespace MsDelivery.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken cancellationToken = default);
}
