using Finitech.BuildingBlocks.SharedKernel.Money;
using Xunit;

namespace Finitech.UnitTests;

public class MoneyTests
{
    [Theory]
    [InlineData(1000, "MAD", 10.00)]
    [InlineData(5000, "EUR", 50.00)]
    [InlineData(100, "USD", 1.00)]
    public void Money_Should_Convert_MinorUnits_To_Decimal(long minorUnits, string currencyCode, decimal expectedDecimal)
    {
        // Act
        var money = new Money(minorUnits, currencyCode);

        // Assert
        Assert.Equal(minorUnits, money.AmountMinorUnits);
        Assert.Equal(expectedDecimal, money.AmountDecimal);
        Assert.Equal(currencyCode, money.Currency.Code);
    }

    [Theory]
    [InlineData(1000, 500, 1500)]
    [InlineData(5000, 2500, 7500)]
    public void Money_Add_Should_Work_Same_Currency(long amount1, long amount2, long expected)
    {
        // Arrange
        var money1 = new Money(amount1, "MAD");
        var money2 = new Money(amount2, "MAD");

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(expected, result.AmountMinorUnits);
        Assert.Equal("MAD", result.Currency.Code);
    }

    [Fact]
    public void Money_Add_Should_Throw_For_Different_Currencies()
    {
        // Arrange
        var money1 = new Money(1000, "MAD");
        var money2 = new Money(1000, "EUR");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
    }

    [Theory]
    [InlineData(1000, 500, 500)]
    [InlineData(5000, 2000, 3000)]
    public void Money_Subtract_Should_Work_Same_Currency(long amount1, long amount2, long expected)
    {
        // Arrange
        var money1 = new Money(amount1, "MAD");
        var money2 = new Money(amount2, "MAD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.Equal(expected, result.AmountMinorUnits);
    }

    [Fact]
    public void Money_Subtract_Should_Throw_For_Insufficient_Amount()
    {
        // Arrange
        var money1 = new Money(500, "MAD");
        var money2 = new Money(1000, "MAD");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => money1.Subtract(money2));
    }

    [Theory]
    [InlineData(1000, 2.5, 2500)]
    [InlineData(5000, 0.5, 2500)]
    public void Money_Multiply_Should_Work(long amount, decimal factor, long expected)
    {
        // Arrange
        var money = new Money(amount, "MAD");

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.Equal(expected, result.AmountMinorUnits);
    }

    [Theory]
    [InlineData("MAD", 504, 2)]
    [InlineData("EUR", 978, 2)]
    [InlineData("USD", 840, 2)]
    public void Currency_Should_Have_Correct_Numeric_Code(string code, int expectedNumeric, int expectedDecimalPlaces)
    {
        // Act
        var currency = Currency.FromCode(code);

        // Assert
        Assert.Equal(code, currency.Code);
        Assert.Equal(expectedNumeric, currency.NumericCode);
        Assert.Equal(expectedDecimalPlaces, currency.DecimalPlaces);
    }

    [Theory]
    [InlineData(504, "MAD")]
    [InlineData(978, "EUR")]
    [InlineData(840, "USD")]
    public void Currency_FromNumericCode_Should_Work(int numericCode, string expectedCode)
    {
        // Act
        var currency = Currency.FromNumericCode(numericCode);

        // Assert
        Assert.Equal(expectedCode, currency.Code);
        Assert.Equal(numericCode, currency.NumericCode);
    }

    [Fact]
    public void SignedMoney_Can_Be_Negative()
    {
        // Arrange & Act
        var signedMoney = new SignedMoney(-1000, Currency.FromCode("MAD"));

        // Assert
        Assert.True(signedMoney.IsNegative);
        Assert.Equal(-1000, signedMoney.AmountMinorUnits);
        Assert.Equal(-10.00m, signedMoney.AmountDecimal);
    }

    [Fact]
    public void SignedMoney_Negate_Should_Change_Sign()
    {
        // Arrange
        var signedMoney = new SignedMoney(1000, Currency.FromCode("MAD"));

        // Act
        var negated = signedMoney.Negate();

        // Assert
        Assert.Equal(-1000, negated.AmountMinorUnits);
        Assert.True(negated.IsNegative);
    }

    [Fact]
    public void Money_Zero_Should_Create_Zero_Amount()
    {
        // Act
        var zero = Money.Zero("MAD");

        // Assert
        Assert.Equal(0, zero.AmountMinorUnits);
        Assert.Equal(0m, zero.AmountDecimal);
        Assert.Equal("MAD", zero.Currency.Code);
    }

    [Fact]
    public void SignedMoney_Addition_Should_Work()
    {
        // Arrange
        var money1 = new SignedMoney(1000, Currency.FromCode("MAD"));
        var money2 = new SignedMoney(-500, Currency.FromCode("MAD"));

        // Act
        var result = money1 + money2;

        // Assert
        Assert.Equal(500, result.AmountMinorUnits);
    }

    [Fact]
    public void SignedMoney_Subtraction_Should_Work()
    {
        // Arrange
        var money1 = new SignedMoney(1000, Currency.FromCode("MAD"));
        var money2 = new SignedMoney(300, Currency.FromCode("MAD"));

        // Act
        var result = money1 - money2;

        // Assert
        Assert.Equal(700, result.AmountMinorUnits);
    }
}
