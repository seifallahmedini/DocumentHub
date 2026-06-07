namespace DocumentHub.Core.Interfaces;

public interface IFileStore
{
    Task<string> SaveAsync(Guid tenantId, string fileName, Stream content, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
    Task<(Stream Content, string MimeType)> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}
