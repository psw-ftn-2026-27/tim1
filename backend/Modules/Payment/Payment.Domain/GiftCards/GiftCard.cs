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
            throw new DomainException("You cannot buy a gift card for yourself.");
        }

        if (amount % 1 != 0 || amount < MinAmount || amount > MaxAmount)
        {
            throw new DomainException($"The gift card amount must be a whole number from {MinAmount} to {MaxAmount} EC.");
        }

        message ??= "";
        if (message.Length > MaxMessageLength)
        {
            throw new DomainException($"The gift card message can have at most {MaxMessageLength} characters.");
        }

        CreatorId = creatorId;
        RecipientId = recipientId;
        Amount = amount;
        Message = message;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CreatedAt.AddYears(ValidYears);
    }

    /// <summary>
    /// Spends the card for its recipient, whole. A card meant for someone else, used or expired is
    /// refused. The moment is a parameter, the way <see cref="StatusAt"/> takes it, so expiry can be tested.
    /// </summary>
    public void Use(Guid buyerId, DateTime now)
    {
        EnsureUsableBy(buyerId, now);

        UsedAt = now;
    }

    /// <summary>Refuses a card meant for someone else, used or expired, without spending it.</summary>
    public void EnsureUsableBy(Guid buyerId, DateTime now)
    {
        if (buyerId != RecipientId)
        {
            throw new DomainException("The gift card was not given to you.");
        }

        if (StatusAt(now) != GiftCardStatus.Valid)
        {
            throw new DomainException("The gift card is not valid: it is already used or has expired.");
        }
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
