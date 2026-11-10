using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.WithdrawalRequests;

public sealed class WithdrawalRequest : AggregateRoot
{
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public WithdrawalRequestStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private WithdrawalRequest() { }

    public WithdrawalRequest(Guid walletId, decimal amount) : base(Guid.NewGuid())
    {
        if (amount <= 0)
        {
            throw new DomainException("A withdrawal request requires an amount greater than zero.");
        }

        WalletId = walletId;
        Amount = amount;
        Status = WithdrawalRequestStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != WithdrawalRequestStatus.Pending)
        {
            throw new DomainException("A withdrawal request that has been decided cannot be decided again.");
        }

        Status = WithdrawalRequestStatus.Approved;
    }

    public void Reject(string reason)
    {
        if (Status != WithdrawalRequestStatus.Pending)
        {
            throw new DomainException("A withdrawal request that has been decided cannot be decided again.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("A rejected withdrawal request requires a reason.");
        }

        Status = WithdrawalRequestStatus.Rejected;
        RejectionReason = reason;
    }
}
