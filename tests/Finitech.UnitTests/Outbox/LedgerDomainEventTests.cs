using Finitech.Modules.Ledger.Domain.Events;
using Xunit;

namespace Finitech.UnitTests.Outbox;

public class LedgerDomainEventTests
{
    [Fact]
    public void LedgerEntryCreatedEvent_Should_Contain_All_Required_Properties()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var currencyCode = "MAD";
        var amountMinorUnits = 100000L;
        var entryType = "Credit";
        var runningBalance = 500000L;
        var reference = "REF-123";

        // Act
        var domainEvent = new LedgerEntryCreatedEvent(
            entryId,
            accountId,
            currencyCode,
            amountMinorUnits,
            entryType,
            runningBalance,
            reference);

        // Assert
        Assert.Equal(entryId, domainEvent.EntryId);
        Assert.Equal(accountId, domainEvent.AccountId);
        Assert.Equal(currencyCode, domainEvent.CurrencyCode);
        Assert.Equal(amountMinorUnits, domainEvent.AmountMinorUnits);
        Assert.Equal(entryType, domainEvent.EntryType);
        Assert.Equal(runningBalance, domainEvent.RunningBalance);
        Assert.Equal(reference, domainEvent.Reference);
        Assert.Equal("LedgerEntryCreatedEvent", domainEvent.EventType);
    }

    [Fact]
    public void LedgerEntryCreatedEvent_Should_Accept_Null_Reference()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        // Act
        var domainEvent = new LedgerEntryCreatedEvent(
            entryId,
            accountId,
            "EUR",
            50000L,
            "Debit",
            100000L,
            null);

        // Assert
        Assert.Null(domainEvent.Reference);
    }

    [Fact]
    public void AccountBalanceUpdatedEvent_Should_Contain_Balance_Change_Details()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var currencyCode = "USD";
        var previousBalance = 100000L;
        var newBalance = 150000L;
        var changeAmount = 50000L;

        // Act
        var domainEvent = new AccountBalanceUpdatedEvent(
            accountId,
            currencyCode,
            previousBalance,
            newBalance,
            changeAmount);

        // Assert
        Assert.Equal(accountId, domainEvent.AccountId);
        Assert.Equal(currencyCode, domainEvent.CurrencyCode);
        Assert.Equal(previousBalance, domainEvent.PreviousBalance);
        Assert.Equal(newBalance, domainEvent.NewBalance);
        Assert.Equal(changeAmount, domainEvent.ChangeAmount);
        Assert.Equal("AccountBalanceUpdatedEvent", domainEvent.EventType);
    }

    [Fact]
    public void LedgerEntryVoidedEvent_Should_Contain_Void_Details()
    {
        // Arrange
        var originalEntryId = Guid.NewGuid();
        var voidEntryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var reason = "Customer request";

        // Act
        var domainEvent = new LedgerEntryVoidedEvent(
            originalEntryId,
            voidEntryId,
            accountId,
            reason);

        // Assert
        Assert.Equal(originalEntryId, domainEvent.OriginalEntryId);
        Assert.Equal(voidEntryId, domainEvent.VoidEntryId);
        Assert.Equal(accountId, domainEvent.AccountId);
        Assert.Equal(reason, domainEvent.Reason);
        Assert.Equal("LedgerEntryVoidedEvent", domainEvent.EventType);
    }
}
