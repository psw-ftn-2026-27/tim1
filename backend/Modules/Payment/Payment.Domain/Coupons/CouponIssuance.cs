using Payment.Domain.Products;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

/// <summary>
/// Makes a coupon for an owner, but only over what the owner sells. The coupon alone cannot tell,
/// because it never sees the products of its owner, so the rule lives here and not in the
/// application service.
/// </summary>
public sealed class CouponIssuance
{
    public Coupon Issue(
        Guid ownerId,
        Guid? productId,
        IReadOnlyList<Product> ownedProducts,
        int percent,
        DateTime expiresAt)
    {
        if (ownedProducts.Count == 0)
        {
            throw new DomainException("Only an owner of a product can make a coupon.");
        }

        if (productId is not null && ownedProducts.All(product => product.Id != productId))
        {
            throw new DomainException("A coupon can only be tied to a product of its owner.");
        }

        return new Coupon(ownerId, productId, percent, expiresAt);
    }
}
