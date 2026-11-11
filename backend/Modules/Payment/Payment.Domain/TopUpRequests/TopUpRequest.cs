using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.TopUpRequests;

public sealed class TopUpRequest : AggregateRoot
{
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public TopUpRequestStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

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
}
