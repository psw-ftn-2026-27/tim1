using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.GiftCards;

public sealed class GiftCard : AggregateRoot
{
    private const decimal MinAmount = 10;
    private const decimal MaxAmount = 1000;
    private const int MaxMessageLength = 200;
    private const int ValidYears = 3;

    public Guid CreatorId { get; private set; }
    public Guid RecipientId { get; private set; }
    public decimal Amount { get; private set; }
    public string Message { get; private set; } = "";
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private GiftCard() { }

    public GiftCard(Guid creatorId, Guid recipientId, decimal amount, string message) : base(Guid.NewGuid())
    {
        if (creatorId == recipientId)
        {
            throw new DomainException("A gift card cannot be bought for yourself.");
        }

        if (amount % 1 != 0 || amount < MinAmount || amount > MaxAmount)
        {
            throw new DomainException($"A gift card amount must be a whole number from {MinAmount} to {MaxAmount} EC.");
        }

        message ??= "";
        if (message.Length > MaxMessageLength)
        {
            throw new DomainException($"A gift card message can have at most {MaxMessageLength} characters.");
        }

        CreatorId = creatorId;
        RecipientId = recipientId;
        Amount = amount;
        Message = message;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CreatedAt.AddYears(ValidYears);
    }

    public GiftCardStatus StatusAt(DateTime now)
    {
        if (UsedAt is not null)
        {
            return GiftCardStatus.Used;
        }

        return now > ExpiresAt ? GiftCardStatus.Expired : GiftCardStatus.Valid;
    }
}
