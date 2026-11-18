using Payment.Domain.Products;
using Shared.Domain;

namespace Payment.Domain.Purchases;

/// <summary>
/// The proof that a customer paid for one entity, and the right of access that comes with it.
/// The purchase keeps the price and the moment of the payment, so a later change of the product
/// leaves what was bought alone. Nothing about a purchase ever changes.
/// </summary>
public sealed class Purchase : AggregateRoot
{
    public Guid BuyerId { get; private set; }
    public ProductReference Reference { get; private set; }
    public decimal Price { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    /// <summary>The coupon that lowered the price of this item, when one did.</summary>
    public Guid? CouponId { get; private set; }

    /// <summary>What the coupon took off this item; Price is already the lowered price.</summary>
    public decimal CouponDiscount { get; private set; }

    private Purchase()
    {
        Reference = null!;
    }

    public Purchase(
        Guid buyerId,
        ProductReference reference,
        decimal price,
        Guid? couponId = null,
        decimal couponDiscount = 0) : base(Guid.NewGuid())
    {
        BuyerId = buyerId;
        Reference = reference;
        Price = price;
        PurchasedAt = DateTime.UtcNow;
        CouponId = couponId;
        CouponDiscount = couponDiscount;
    }
}
