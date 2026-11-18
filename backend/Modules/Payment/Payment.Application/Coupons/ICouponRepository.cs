using Payment.Domain.Coupons;

namespace Payment.Application.Coupons;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code);
    void Add(Coupon coupon);
}
