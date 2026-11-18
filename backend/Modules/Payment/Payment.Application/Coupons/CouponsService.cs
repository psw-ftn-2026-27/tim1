using Payment.Application.Products;
using Payment.Domain.Coupons;

namespace Payment.Application.Coupons;

public sealed class CouponsService
{
    private readonly ICouponRepository _couponRepository;
    private readonly IProductRepository _productRepository;
    private readonly CouponIssuance _couponIssuance;
    private readonly IUnitOfWork _unitOfWork;

    public CouponsService(
        ICouponRepository couponRepository,
        IProductRepository productRepository,
        CouponIssuance couponIssuance,
        IUnitOfWork unitOfWork)
    {
        _couponRepository = couponRepository;
        _productRepository = productRepository;
        _couponIssuance = couponIssuance;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateCouponAsync(Guid ownerId, CreateCouponDto dto)
    {
        var ownedProducts = await _productRepository.GetByOwnerAsync(ownerId);

        var coupon = _couponIssuance.Issue(ownerId, dto.ProductId, ownedProducts, dto.Percent, dto.ExpiresAt);

        _couponRepository.Add(coupon);
        await _unitOfWork.SaveChangesAsync();
    }
}
