using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Wallet : AggregateRoot
{
    private readonly List<Transaction> _transactions = [];

    public Guid OwnerId { get; private set; }
    public decimal AvailableBalance { get; private set; }
    public decimal ReservedBalance { get; private set; }
    public IReadOnlyList<Transaction> Transactions => _transactions;

    private Wallet() { }

    public Wallet(Guid ownerId) : base(Guid.NewGuid())
    {
        OwnerId = ownerId;
        AvailableBalance = 0;
        ReservedBalance = 0;
    }
}
