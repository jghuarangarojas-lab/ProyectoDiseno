using MsServices.Domain.Enums;

namespace MsServices.Domain.Entities;

public class ProjectAttachment
{
    public Guid AttachmentId { get; set; }
    public Guid ProjectId { get; set; }
    public ServiceProject? Project { get; set; }
    public string FileUrl { get; set; } = null!;
    public AttachmentFileType FileType { get; set; }
    public string FileName { get; set; } = null!;
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}
