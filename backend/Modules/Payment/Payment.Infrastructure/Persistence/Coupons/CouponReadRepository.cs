using Microsoft.EntityFrameworkCore;
using Payment.Application.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponReadRepository : ICouponReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public CouponReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<CouponDto>> GetCouponsOfOwnerAsync(Guid ownerId)
    {
        return _dbContext.Coupons
            .Where(coupon => coupon.OwnerId == ownerId)
            .OrderByDescending(coupon => coupon.CreatedAt)
            .Select(coupon => new CouponDto(
                coupon.Id,
                coupon.Code,
                coupon.Percent,
                coupon.ExpiresAt,
                _dbContext.Products
                    .Where(product => product.Id == coupon.ProductId)
                    .Select(product => product.Name)
                    .FirstOrDefault(),
                coupon.UsedAt != null))
            .ToListAsync();
    }

    public Task<List<OwnedProductDto>> GetOwnedProductsAsync(Guid ownerId)
    {
        return _dbContext.Products
            .Where(product => product.OwnerId == ownerId)
            .OrderBy(product => product.Name)
            .Select(product => new OwnedProductDto(product.Id, product.Name))
            .ToListAsync();
    }
}
