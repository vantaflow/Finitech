using Finitech.BuildingBlocks.Domain.Outbox;
using Finitech.BuildingBlocks.SharedKernel.Primitives;
using Xunit;

namespace Finitech.UnitTests.Outbox;

public class OutboxMessageTests
{
    [Fact]
    public void OutboxMessage_Should_Be_Created_With_Default_Pending_Status()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "TestEvent",
            Payload = "{}",
            OccurredAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(0, message.RetryCount);
        Assert.Null(message.ProcessedAt);
        Assert.Null(message.Error);
    }

    [Fact]
    public void OutboxMessage_Should_Store_Event_Details()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var correlationId = "corr-123";
        var payload = "{\"test\": \"value\"}";

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "TestEvent",
            Payload = payload,
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId
        };

        // Assert
        Assert.Equal("TestEvent", message.EventType);
        Assert.Equal(payload, message.Payload);
        Assert.Equal(correlationId, message.CorrelationId);
    }
}

public sealed record TestDomainEvent : DomainEvent
{
    public string TestProperty { get; init; } = string.Empty;
    public int NumberValue { get; init; }

    public TestDomainEvent(string testProperty, int numberValue)
    {
        TestProperty = testProperty;
        NumberValue = numberValue;
    }
}

public class DomainEventSerializationTests
{
    [Fact]
    public void DomainEvent_Should_Serialize_To_Json()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("test", 42);

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        // Assert
        Assert.Contains("\"eventType\":\"TestDomainEvent\"", json);
        Assert.Contains("\"testProperty\":\"test\"", json);
        Assert.Contains("\"numberValue\":42", json);
    }

    [Fact]
    public void DomainEvent_Should_Deserialize_From_Json()
    {
        // Arrange
        var originalEvent = new TestDomainEvent("test", 42);
        var json = System.Text.Json.JsonSerializer.Serialize(originalEvent, originalEvent.GetType(), new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        // Act
        var deserializedEvent = System.Text.Json.JsonSerializer.Deserialize<TestDomainEvent>(json, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        // Assert
        Assert.NotNull(deserializedEvent);
        Assert.Equal("test", deserializedEvent.TestProperty);
        Assert.Equal(42, deserializedEvent.NumberValue);
        Assert.Equal("TestDomainEvent", deserializedEvent.EventType);
    }
}
