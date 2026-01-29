namespace Finitech.Modules.Statements.Contracts.DTOs;

public record StatementDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string StatementType { get; init; } = string.Empty; // Monthly, Quarterly, Annual, Custom
    public string CurrencyCode { get; init; } = string.Empty;
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public long OpeningBalanceMinorUnits { get; init; }
    public long ClosingBalanceMinorUnits { get; init; }
    public decimal OpeningBalanceDecimal { get; init; }
    public decimal ClosingBalanceDecimal { get; init; }
    public long TotalCreditsMinorUnits { get; init; }
    public long TotalDebitsMinorUnits { get; init; }
    public List<StatementTransactionDto> Transactions { get; init; } = new();
    public DateTime GeneratedAt { get; init; }
    public string Status { get; init; } = string.Empty; // Generated, Downloaded, Archived
}

public record StatementTransactionDto
{
    public Guid TransactionId { get; init; }
    public DateTime TransactionDate { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public string EntryType { get; init; } = string.Empty; // Credit, Debit
    public long AmountMinorUnits { get; init; }
    public decimal AmountDecimal { get; init; }
    public long RunningBalanceMinorUnits { get; init; }
}

public record GenerateStatementRequest
{
    public Guid AccountId { get; init; }
    public string StatementType { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
}

public record ConsolidatedStatementDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public List<StatementSectionDto> Sections { get; init; } = new();
    public DateTime GeneratedAt { get; init; }
}

public record StatementSectionDto
{
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public StatementSummaryDto Summary { get; init; } = new();
    public List<StatementTransactionDto> Transactions { get; init; } = new();
}

public record StatementSummaryDto
{
    public long OpeningBalanceMinorUnits { get; init; }
    public long ClosingBalanceMinorUnits { get; init; }
    public long TotalCreditsMinorUnits { get; init; }
    public long TotalDebitsMinorUnits { get; init; }
    public int TransactionCount { get; init; }
}

public record StatementExportRequest
{
    public Guid StatementId { get; init; }
    public string Format { get; init; } = string.Empty; // PDF, CSV, Excel
}

public record StatementExportDto
{
    public Guid StatementId { get; init; }
    public string Format { get; init; } = string.Empty;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string ContentType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
