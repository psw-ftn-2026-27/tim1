using Shared.Domain;

namespace Payment.Application.Coupons;

public sealed class CouponsQueries
{
    private readonly ICouponReadRepository _couponReadRepository;

    public CouponsQueries(ICouponReadRepository couponReadRepository)
    {
        _couponReadRepository = couponReadRepository;
    }

    public Task<PageResult<CouponDto>> GetCouponsAsync(Guid ownerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _couponReadRepository.GetCouponsOfOwnerAsync(ownerId, page, pageSize);
    }

    public Task<List<OwnedProductDto>> GetOwnedProductsAsync(Guid ownerId)
    {
        return _couponReadRepository.GetOwnedProductsAsync(ownerId);
    }
}
