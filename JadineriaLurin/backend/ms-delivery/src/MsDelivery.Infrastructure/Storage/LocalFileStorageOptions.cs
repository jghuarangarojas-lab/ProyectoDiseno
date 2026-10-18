namespace MsDelivery.Infrastructure.Storage;

public sealed class LocalFileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = "storage";
    public string BaseUrl { get; set; } = "/files";
}
