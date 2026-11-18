using Payment.Domain.Products;
using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    public DateTime OccurredAt { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }

    /// <summary>Which product the money was for, or <c>null</c> when no product was involved.</summary>
    public ProductReference? Reference { get; private set; }

    /// <summary>
    /// The name the product had when the money moved, so the history keeps showing what was
    /// paid for even after the product is renamed. <c>null</c> when no product was involved.
    /// </summary>
    public string? ProductName { get; private set; }

    private Transaction() { }

    public Transaction(TransactionType type, decimal amount, decimal balanceAfter) : base(Guid.NewGuid())
    {
        OccurredAt = DateTime.UtcNow;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
    }

    public Transaction(
        TransactionType type,
        decimal amount,
        decimal balanceAfter,
        ProductReference reference,
        string productName) : this(type, amount, balanceAfter)
    {
        Reference = reference;
        ProductName = productName;
    }
}
