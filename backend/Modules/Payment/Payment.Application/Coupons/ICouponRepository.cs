using Payment.Domain.Coupons;

namespace Payment.Application.Coupons;

public interface ICouponRepository
{
    void Add(Coupon coupon);
}
