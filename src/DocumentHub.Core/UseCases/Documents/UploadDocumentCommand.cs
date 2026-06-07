using DocumentHub.Core.Entities;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Documents;

public record UploadDocumentCommand(
    string Name,
    List<string> Tags,
    string FileName,
    string MimeType,
    Stream Content,
    long FileSize,
    Guid CallerUserId,
    Guid CallerTenantId);

public record UploadDocumentResult(Guid Id);

public class UploadDocumentHandler(IRepository<Document> documents, IFileStore fileStore)
{
    public async Task<UploadDocumentResult> HandleAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var filePath = await fileStore.SaveAsync(command.CallerTenantId, command.FileName, command.Content, cancellationToken);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            TenantId = command.CallerTenantId,
            UploadedById = command.CallerUserId,
            Name = command.Name,
            FilePath = filePath,
            MimeType = command.MimeType,
            FileSize = command.FileSize,
            Tags = command.Tags,
            UploadedAt = DateTime.UtcNow
        };

        await documents.AddAsync(document, cancellationToken);
        return new UploadDocumentResult(document.Id);
    }
}
