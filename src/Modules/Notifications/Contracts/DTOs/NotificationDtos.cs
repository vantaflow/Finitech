namespace Finitech.Modules.Notifications.Contracts.DTOs;

public record SendNotificationRequest
{
    public Guid RecipientPartyId { get; init; }
    public string NotificationType { get; init; } = string.Empty; // Transaction, KYC, Limit, Security
    public string Channel { get; init; } = string.Empty; // Email, SMS, Push, InApp
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Dictionary<string, string> Data { get; init; } = new();
}

public record SendTemplatedNotificationRequest
{
    public Guid RecipientPartyId { get; init; }
    public string TemplateCode { get; init; } = string.Empty;
    public List<string> Channels { get; init; } = new(); // Email, SMS, Push, InApp
    public Dictionary<string, string> TemplateData { get; init; } = new();
}

public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid RecipientPartyId { get; init; }
    public string NotificationType { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // Pending, Sent, Failed, Read
    public DateTime CreatedAt { get; init; }
    public DateTime? SentAt { get; init; }
    public DateTime? ReadAt { get; init; }
}

public record NotificationPreferenceDto
{
    public Guid PartyId { get; init; }
    public string NotificationType { get; init; } = string.Empty;
    public bool EmailEnabled { get; init; }
    public bool SmsEnabled { get; init; }
    public bool PushEnabled { get; init; }
    public bool InAppEnabled { get; init; } = true;
}

public record UpdateNotificationPreferenceRequest
{
    public Guid PartyId { get; init; }
    public string NotificationType { get; init; } = string.Empty;
    public bool? EmailEnabled { get; init; }
    public bool? SmsEnabled { get; init; }
    public bool? PushEnabled { get; init; }
    public bool? InAppEnabled { get; init; }
}

public record NotificationTemplateDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string EmailSubjectTemplate { get; init; } = string.Empty;
    public string EmailBodyTemplate { get; init; } = string.Empty;
    public string? SmsTemplate { get; init; }
    public string? PushTemplate { get; init; }
    public string? InAppTemplate { get; init; }
}
