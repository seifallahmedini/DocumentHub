using DocumentHub.Core.Entities;
using DocumentHub.Core.Exceptions;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Documents;

public record DeleteDocumentCommand(Guid DocumentId, Guid CallerTenantId);

public class DeleteDocumentHandler(IRepository<Document> documents, IFileStore fileStore)
{
    public async Task HandleAsync(DeleteDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var document = await documents.FirstOrDefaultAsync(
            d => d.Id == command.DocumentId && d.TenantId == command.CallerTenantId, cancellationToken)
            ?? throw new NotFoundException("Document not found.");

        await fileStore.DeleteAsync(document.FilePath, cancellationToken);
        await documents.DeleteAsync(document, cancellationToken);
    }
}
