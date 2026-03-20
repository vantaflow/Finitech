namespace Finitech.Modules.Notifications.Application.Services;

public interface INotificationSender
{
    Task SendSmsAsync(string phoneNumber, string body);
    Task SendEmailAsync(string email, string subject, string body);
    Task SendPushAsync(string deviceToken, string title, string body);
}

public class NotificationApplicationService
{
    private readonly INotificationSender? _sender;

    public NotificationApplicationService(INotificationSender? sender = null) => _sender = sender;

    public async Task SendAsync(string channel, string recipient, string subject, string body)
    {
        if (_sender == null)
        {
            Console.WriteLine($"[{channel} → {recipient}] {subject}: {body}");
            return;
        }

        switch (channel.ToUpperInvariant())
        {
            case "SMS": await _sender.SendSmsAsync(recipient, body); break;
            case "EMAIL": await _sender.SendEmailAsync(recipient, subject, body); break;
            case "PUSH": await _sender.SendPushAsync(recipient, subject, body); break;
        }
    }
}
