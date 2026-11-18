namespace Payment.Application.Coupons;

public interface ICouponReadRepository
{
    Task<List<CouponDto>> GetCouponsOfOwnerAsync(Guid ownerId);
    Task<List<OwnedProductDto>> GetOwnedProductsAsync(Guid ownerId);
}
