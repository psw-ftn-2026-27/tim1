using System.Security.Cryptography;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

/// <summary>
/// A discount a product owner hands out. It is tied to one product of the owner, or to any of
/// them when no product is named. The code is generated here, so a coupon cannot exist without
/// one; that the code is unique is the database guarantee, because no aggregate sees the others.
/// </summary>
public sealed class Coupon : AggregateRoot
{
    private const int CodeLength = 8;
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public Guid OwnerId { get; private set; }
    public Guid? ProductId { get; private set; }
    public int Percent { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public string Code { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private Coupon()
    {
        Code = null!;
    }

    public Coupon(Guid ownerId, Guid? productId, int percent, DateTime expiresAt) : base(Guid.NewGuid())
    {
        if (percent < 1 || percent > 100)
        {
            throw new DomainException("The discount of a coupon is a whole number from 1 to 100 percent.");
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new DomainException("A coupon must expire in the future.");
        }

        OwnerId = ownerId;
        ProductId = productId;
        Percent = percent;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        Code = RandomNumberGenerator.GetString(CodeAlphabet, CodeLength);
    }
}
