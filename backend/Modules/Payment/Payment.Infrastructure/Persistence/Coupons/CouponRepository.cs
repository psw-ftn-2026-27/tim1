using Microsoft.EntityFrameworkCore;
using Payment.Application.Coupons;
using Payment.Domain.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponRepository : ICouponRepository
{
    private readonly PaymentDbContext _dbContext;

    public CouponRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Coupon?> GetByCodeAsync(string code)
    {
        return _dbContext.Coupons.FirstOrDefaultAsync(coupon => coupon.Code == code);
    }

    public void Add(Coupon coupon)
    {
        _dbContext.Coupons.Add(coupon);
    }
}
