using Payment.Domain.Coupons;

namespace Payment.Tests.Integration.Seeds;

internal static class CouponSeed
{
    /// <summary>A coupon of the owner of the demo tour, tied to that tour.</summary>
    public static readonly Coupon DemoExplorersTourCoupon = new(
        DemoUsers.Explorer,
        ProductSeed.DemoExplorersTour.Id,
        20,
        DateTime.UtcNow.AddDays(30));

    /// <summary>A coupon of the same owner that is valid for any of their products.</summary>
    public static readonly Coupon DemoExplorersAnyProductCoupon = new(
        DemoUsers.Explorer,
        null,
        10,
        DateTime.UtcNow.AddDays(60));

    /// <summary>A coupon of another owner, which the demo owner must never see.</summary>
    public static readonly Coupon ForgottenOwnersCoupon = new(
        ProductSeed.TourOfAForgottenOwner.OwnerId,
        ProductSeed.TourOfAForgottenOwner.Id,
        50,
        DateTime.UtcNow.AddDays(30));

    public static Coupon[] All => [DemoExplorersTourCoupon, DemoExplorersAnyProductCoupon, ForgottenOwnersCoupon];
}
