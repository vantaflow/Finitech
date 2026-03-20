using Finitech.BuildingBlocks.Domain.Outbox;

namespace Finitech.BuildingBlocks.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqSettings _settings;

    public RabbitMqEventPublisher(RabbitMqSettings settings) => _settings = settings;

    public Task PublishAsync(OutboxMessage message)
    {
        var exchange = GetExchangeName(message.EventType);
        Console.WriteLine($"[RabbitMQ] Publishing to {exchange}: {message.EventType} (ID: {message.Id})");
        return Task.CompletedTask;
    }

    public async Task PublishBatchAsync(IEnumerable<OutboxMessage> messages)
    {
        foreach (var m in messages) await PublishAsync(m);
    }

    private static string GetExchangeName(string eventType) => eventType switch
    {
        var t when t.StartsWith("Ledger") => "finitech.ledger",
        var t when t.StartsWith("Payment") => "finitech.payments",
        var t when t.StartsWith("Wallet") => "finitech.wallet",
        _ => "finitech.default"
    };
}

public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "finitech";
    public string Password { get; set; } = "finitech";
}
