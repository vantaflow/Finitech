namespace Finitech.Modules.Wallet.Domain;

/// <summary>
/// Facture payable via le wallet.
/// Couvre : Maroc Telecom, Inwi, Orange, REDAL, LYDEC, ONEE, impôts.
/// </summary>
public class Bill
{
    public Guid Id { get; set; }
    public string BillerName { get; set; } = string.Empty;
    public string BillerReference { get; set; } = string.Empty;
    public string CustomerReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string BillType { get; set; } = string.Empty; // Telecom, Eau, Électricité, Taxes
    public DateTime CreatedAt { get; set; }
}
