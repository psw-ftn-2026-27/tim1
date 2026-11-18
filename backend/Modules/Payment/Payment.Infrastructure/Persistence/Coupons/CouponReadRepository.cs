using Microsoft.EntityFrameworkCore;
using Payment.Application.Coupons;
using Payment.Domain.Coupons;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponReadRepository : ICouponReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public CouponReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // The status mirrors Coupon.StatusAt, because a list is projected and never loads aggregates.
    public async Task<PageResult<CouponDto>> GetCouponsOfOwnerAsync(Guid ownerId, int page, int pageSize)
    {
        var now = DateTime.UtcNow;
        var coupons = _dbContext.Coupons.Where(coupon => coupon.OwnerId == ownerId);
        var totalCount = await coupons.CountAsync();

        var items = await coupons
            .OrderByDescending(coupon => coupon.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(coupon => new CouponDto(
                coupon.Id,
                coupon.Code,
                coupon.Percent,
                coupon.ExpiresAt,
                _dbContext.Products
                    .Where(product => product.Id == coupon.ProductId)
                    .Select(product => product.Name)
                    .FirstOrDefault(),
                coupon.UsedAt != null ? CouponStatus.Used
                    : coupon.ExpiresAt < now ? CouponStatus.Expired
                    : CouponStatus.Valid))
            .ToListAsync();

        return new PageResult<CouponDto>(items, totalCount);
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
