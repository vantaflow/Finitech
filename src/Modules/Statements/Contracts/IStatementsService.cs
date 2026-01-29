using Finitech.Modules.Statements.Contracts.DTOs;

namespace Finitech.Modules.Statements.Contracts;

public interface IStatementsService
{
    Task<StatementDto> GenerateStatementAsync(GenerateStatementRequest request, CancellationToken cancellationToken = default);
    Task<StatementDto?> GetStatementAsync(Guid statementId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatementDto>> GetStatementsByAccountAsync(Guid accountId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<ConsolidatedStatementDto> GenerateConsolidatedStatementAsync(Guid partyId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<StatementExportDto> ExportStatementAsync(StatementExportRequest request, CancellationToken cancellationToken = default);
    Task GenerateMonthlyStatementsAsync(CancellationToken cancellationToken = default);
}
