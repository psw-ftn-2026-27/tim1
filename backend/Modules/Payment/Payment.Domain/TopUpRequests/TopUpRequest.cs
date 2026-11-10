using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.TopUpRequests;

public sealed class TopUpRequest : AggregateRoot
{
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public TopUpRequestStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private TopUpRequest() { }

    public TopUpRequest(Guid walletId, decimal amount) : base(Guid.NewGuid())
    {
        if (amount <= 0)
        {
            throw new DomainException("A top-up request requires an amount greater than zero.");
        }

        WalletId = walletId;
        Amount = amount;
        Status = TopUpRequestStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != TopUpRequestStatus.Pending)
        {
            throw new DomainException("A top-up request that has been decided cannot be decided again.");
        }

        Status = TopUpRequestStatus.Approved;
    }

    public void Reject(string reason)
    {
        if (Status != TopUpRequestStatus.Pending)
        {
            throw new DomainException("A top-up request that has been decided cannot be decided again.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("A rejected top-up request requires a reason.");
        }

        Status = TopUpRequestStatus.Rejected;
        RejectionReason = reason;
    }
}
