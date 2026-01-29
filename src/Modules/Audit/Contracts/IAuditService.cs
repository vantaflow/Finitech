using Finitech.Modules.Audit.Contracts.DTOs;

namespace Finitech.Modules.Audit.Contracts;

public interface IAuditService
{
    Task LogAsync(LogAuditRequest request, CancellationToken cancellationToken = default);
    Task<AuditQueryResponse> QueryAsync(AuditQueryRequest request, CancellationToken cancellationToken = default);
}
