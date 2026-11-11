using Shared.Domain;
using Shared.Domain.Exceptions;

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

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("A deposit requires an amount greater than zero.");
        }

        AvailableBalance += amount;
        _transactions.Add(new Transaction(TransactionType.TopUp, amount, AvailableBalance));
    }

    public void ReserveForWithdrawal(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("A withdrawal requires an amount greater than zero.");
        }

        if (amount > AvailableBalance)
        {
            throw new DomainException("A withdrawal cannot exceed the available balance.");
        }

        AvailableBalance -= amount;
        ReservedBalance += amount;
        _transactions.Add(new Transaction(TransactionType.Reservation, amount, AvailableBalance));
    }

    public void PayOutReserved(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("A payout requires an amount greater than zero.");
        }

        if (amount > ReservedBalance)
        {
            throw new DomainException("A payout cannot exceed the reserved balance.");
        }

        ReservedBalance -= amount;
        _transactions.Add(new Transaction(TransactionType.Payout, amount, AvailableBalance));
    }

    public void ReturnReserved(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("A returned reservation requires an amount greater than zero.");
        }

        if (amount > ReservedBalance)
        {
            throw new DomainException("A returned reservation cannot exceed the reserved balance.");
        }

        ReservedBalance -= amount;
        AvailableBalance += amount;
        _transactions.Add(new Transaction(TransactionType.ReservationReturn, amount, AvailableBalance));
    }
}
