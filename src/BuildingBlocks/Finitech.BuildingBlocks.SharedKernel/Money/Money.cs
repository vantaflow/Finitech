using System.Globalization;

namespace Finitech.BuildingBlocks.SharedKernel.Money;

/// <summary>
/// Représentation immutable d'une valeur monétaire avec devise ISO 4217.
/// Stockage en minor units (long) pour éviter les erreurs de précision décimale.
/// </summary>
public readonly record struct Money
{
    public long AmountMinorUnits { get; }
    public Currency Currency { get; }

    public Money(long amountMinorUnits, Currency currency)
    {
        if (amountMinorUnits < 0)
            throw new ArgumentException("Amount cannot be negative. Use explicit type for debts if needed.", nameof(amountMinorUnits));

        AmountMinorUnits = amountMinorUnits;
        Currency = currency;
    }

    public Money(long amountMinorUnits, string currencyCode)
        : this(amountMinorUnits, Currency.FromCode(currencyCode))
    {
    }

    public decimal AmountDecimal => AmountMinorUnits / (decimal)Math.Pow(10, Currency.DecimalPlaces);

    public static Money Zero(Currency currency) => new(0, currency);
    public static Money Zero(string currencyCode) => new(0, currencyCode);

    public Money Add(Money other)
    {
        if (!Currency.Equals(other.Currency))
            throw new InvalidOperationException($"Cannot add {other.Currency.Code} to {Currency.Code}");

        return new Money(AmountMinorUnits + other.AmountMinorUnits, Currency);
    }

    public Money Subtract(Money other)
    {
        if (!Currency.Equals(other.Currency))
            throw new InvalidOperationException($"Cannot subtract {other.Currency.Code} from {Currency.Code}");

        var result = AmountMinorUnits - other.AmountMinorUnits;
        if (result < 0)
            throw new InvalidOperationException("Result would be negative");

        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        var result = (long)Math.Round(AmountMinorUnits * factor);
        return new Money(result, Currency);
    }

    public override string ToString() => $"{AmountDecimal} {Currency.Code}";
}

/// <summary>
/// Devise ISO 4217 avec code alpha et numérique.
/// </summary>
public readonly record struct Currency
{
    public string Code { get; }
    public int NumericCode { get; }
    public int DecimalPlaces { get; }
    public string Name { get; }

    private Currency(string code, int numericCode, int decimalPlaces, string name)
    {
        Code = code;
        NumericCode = numericCode;
        DecimalPlaces = decimalPlaces;
        Name = name;
    }

    public static readonly Currency MAD = new("MAD", 504, 2, "Moroccan Dirham");
    public static readonly Currency EUR = new("EUR", 978, 2, "Euro");
    public static readonly Currency USD = new("USD", 840, 2, "US Dollar");
    public static readonly Currency GBP = new("GBP", 826, 2, "British Pound");

    private static readonly Dictionary<string, Currency> ByCode = new(StringComparer.OrdinalIgnoreCase)
    {
        { "MAD", MAD },
        { "EUR", EUR },
        { "USD", USD },
        { "GBP", GBP }
    };

    private static readonly Dictionary<int, Currency> ByNumericCode = new()
    {
        { 504, MAD },
        { 978, EUR },
        { 840, USD },
        { 826, GBP }
    };

    public static Currency FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code cannot be empty", nameof(code));

        if (ByCode.TryGetValue(code.ToUpperInvariant(), out var currency))
            return currency;

        throw new ArgumentException($"Unknown currency code: {code}", nameof(code));
    }

    public static Currency FromNumericCode(int numericCode)
    {
        if (ByNumericCode.TryGetValue(numericCode, out var currency))
            return currency;

        throw new ArgumentException($"Unknown numeric currency code: {numericCode}", nameof(numericCode));
    }

    public static bool TryFromCode(string code, out Currency currency)
    {
        return ByCode.TryGetValue(code ?? string.Empty, out currency);
    }

    public override string ToString() => Code;
}

/// <summary>
/// Montant signé (peut être négatif) pour les écritures comptables.
/// </summary>
public readonly record struct SignedMoney
{
    public long AmountMinorUnits { get; }
    public Currency Currency { get; }

    public SignedMoney(long amountMinorUnits, Currency currency)
    {
        AmountMinorUnits = amountMinorUnits;
        Currency = currency;
    }

    public decimal AmountDecimal => AmountMinorUnits / (decimal)Math.Pow(10, Currency.DecimalPlaces);
    public bool IsNegative => AmountMinorUnits < 0;
    public bool IsPositive => AmountMinorUnits > 0;
    public bool IsZero => AmountMinorUnits == 0;

    public SignedMoney Negate() => new(-AmountMinorUnits, Currency);

    public Money Absolute()
    {
        if (AmountMinorUnits < 0)
            throw new InvalidOperationException("Cannot convert negative signed money to Money");
        return new Money(AmountMinorUnits, Currency);
    }

    public static SignedMoney operator +(SignedMoney a, SignedMoney b)
    {
        if (!a.Currency.Equals(b.Currency))
            throw new InvalidOperationException($"Cannot add {b.Currency.Code} to {a.Currency.Code}");
        return new SignedMoney(a.AmountMinorUnits + b.AmountMinorUnits, a.Currency);
    }

    public static SignedMoney operator -(SignedMoney a, SignedMoney b)
    {
        if (!a.Currency.Equals(b.Currency))
            throw new InvalidOperationException($"Cannot subtract {b.Currency.Code} from {a.Currency.Code}");
        return new SignedMoney(a.AmountMinorUnits - b.AmountMinorUnits, a.Currency);
    }

    public override string ToString() => $"{AmountDecimal} {Currency.Code}";
}
