using Finitech.Modules.Documents.Contracts.DTOs;

namespace Finitech.Modules.Documents.Contracts;

public interface IDocumentsService
{
    Task<DocumentDto> UploadAsync(UploadDocumentRequest request, CancellationToken cancellationToken = default);
    Task<DocumentContentDto> GetContentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<DocumentDto?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentDto>> ListAsync(ListDocumentsRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid documentId, string deletedBy, CancellationToken cancellationToken = default);
}
