namespace Payment.Application.Coupons;

public sealed class CouponsQueries
{
    private readonly ICouponReadRepository _couponReadRepository;

    public CouponsQueries(ICouponReadRepository couponReadRepository)
    {
        _couponReadRepository = couponReadRepository;
    }

    public Task<List<CouponDto>> GetCouponsAsync(Guid ownerId)
    {
        return _couponReadRepository.GetCouponsOfOwnerAsync(ownerId);
    }

    public Task<List<OwnedProductDto>> GetOwnedProductsAsync(Guid ownerId)
    {
        return _couponReadRepository.GetOwnedProductsAsync(ownerId);
    }
}
