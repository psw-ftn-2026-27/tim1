using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    public DateTime OccurredAt { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }

    private Transaction() { }

    public Transaction(TransactionType type, decimal amount, decimal balanceAfter) : base(Guid.NewGuid())
    {
        OccurredAt = DateTime.UtcNow;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
    }
}
