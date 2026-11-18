using Shared.Domain;

namespace Payment.Application.Coupons;

public interface ICouponReadRepository
{
    Task<PageResult<CouponDto>> GetCouponsOfOwnerAsync(Guid ownerId, int page, int pageSize);
    Task<List<OwnedProductDto>> GetOwnedProductsAsync(Guid ownerId);
}
