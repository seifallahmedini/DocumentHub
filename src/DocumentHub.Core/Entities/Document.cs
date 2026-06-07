namespace DocumentHub.Core.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UploadedById { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public List<string> Tags { get; set; } = [];
    public DateTime UploadedAt { get; set; }
}
