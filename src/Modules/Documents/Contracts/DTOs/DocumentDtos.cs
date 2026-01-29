namespace Finitech.Modules.Documents.Contracts.DTOs;

public record UploadDocumentRequest
{
    public Guid PartyId { get; init; }
    public string DocumentType { get; init; } = string.Empty; // KYC, Contract, Statement, Receipt
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public Guid? RelatedEntityId { get; init; } // AccountId, TransactionId, etc.
    public string? RelatedEntityType { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

public record DocumentDto
{
    public Guid Id { get; init; }
    public Guid PartyId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public int Version { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? RelatedEntityType { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
    public DateTime UploadedAt { get; init; }
    public string UploadedBy { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
}

public record DocumentContentDto
{
    public Guid Id { get; init; }
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string ContentType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}

public record ListDocumentsRequest
{
    public Guid PartyId { get; init; }
    public string? DocumentType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 50;
}
