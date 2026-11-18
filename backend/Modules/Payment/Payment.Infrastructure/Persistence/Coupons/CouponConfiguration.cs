using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal static class CouponConfiguration
{
    public static void ConfigureCoupons(this ModelBuilder modelBuilder)
    {
        ConfigureCoupon(modelBuilder.Entity<Coupon>());
    }

    private static void ConfigureCoupon(EntityTypeBuilder<Coupon> builder)
    {
        builder.Property(coupon => coupon.Code).HasMaxLength(Coupon.CodeLength);
        builder.HasIndex(coupon => coupon.Code).IsUnique();
        builder.HasIndex(coupon => coupon.OwnerId);
    }
}
