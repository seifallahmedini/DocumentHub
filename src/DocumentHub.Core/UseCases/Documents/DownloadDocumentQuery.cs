using DocumentHub.Core.Entities;
using DocumentHub.Core.Exceptions;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Documents;

public record DownloadDocumentQuery(Guid DocumentId, Guid CallerTenantId);

public record DownloadDocumentResult(Stream Content, string MimeType, string FileName);

public class DownloadDocumentHandler(IRepository<Document> documents, IFileStore fileStore)
{
    public async Task<DownloadDocumentResult> HandleAsync(DownloadDocumentQuery query, CancellationToken cancellationToken = default)
    {
        var document = await documents.FirstOrDefaultAsync(
            d => d.Id == query.DocumentId && d.TenantId == query.CallerTenantId, cancellationToken)
            ?? throw new NotFoundException("Document not found.");

        var (content, mimeType) = await fileStore.ReadAsync(document.FilePath, cancellationToken);
        return new DownloadDocumentResult(content, mimeType, document.Name);
    }
}
