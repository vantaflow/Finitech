namespace Finitech.Modules.Notifications.Application.Services;

public class NotificationApplicationService
{
    public async Task SendAsync(Guid recipientPartyId, string channel, string subject, string body, Dictionary<string, string>? data = null)
    {
        // Route to the appropriate channel (SMS, Email, Push)
        switch (channel.ToUpperInvariant())
        {
            case "SMS":
                await SendSmsAsync(recipientPartyId, body);
                break;
            case "EMAIL":
                await SendEmailAsync(recipientPartyId, subject, body);
                break;
            case "PUSH":
                await SendPushAsync(recipientPartyId, subject, body, data);
                break;
            default:
                throw new ArgumentException($"Unknown channel: {channel}");
        }
    }

    private Task SendSmsAsync(Guid recipientPartyId, string body)
    {
        Console.WriteLine($"[SMS → {recipientPartyId}] {body}");
        return Task.CompletedTask;
    }

    private Task SendEmailAsync(Guid recipientPartyId, string subject, string body)
    {
        Console.WriteLine($"[EMAIL → {recipientPartyId}] {subject}: {body}");
        return Task.CompletedTask;
    }

    private Task SendPushAsync(Guid recipientPartyId, string subject, string body, Dictionary<string, string>? data)
    {
        Console.WriteLine($"[PUSH → {recipientPartyId}] {subject}: {body}");
        return Task.CompletedTask;
    }
}
