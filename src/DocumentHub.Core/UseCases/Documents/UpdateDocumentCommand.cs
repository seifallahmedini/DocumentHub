using DocumentHub.Core.Entities;
using DocumentHub.Core.Exceptions;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Documents;

public record UpdateDocumentCommand(Guid DocumentId, string Name, List<string> Tags, Guid CallerTenantId);

public class UpdateDocumentHandler(IRepository<Document> documents)
{
    public async Task HandleAsync(UpdateDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var document = await documents.FirstOrDefaultAsync(
            d => d.Id == command.DocumentId && d.TenantId == command.CallerTenantId, cancellationToken)
            ?? throw new NotFoundException("Document not found.");

        document.Name = command.Name;
        document.Tags = command.Tags;
        await documents.UpdateAsync(document, cancellationToken);
    }
}
