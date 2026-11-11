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
}
