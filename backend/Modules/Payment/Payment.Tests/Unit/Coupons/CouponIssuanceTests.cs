using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Coupons;

public class CouponIssuanceTests
{
    private static Product CreateFortressTour(Guid ownerId) =>
        new(new ProductReference(ProductType.Tour, Guid.Parse("50000000-0000-0000-0000-000000000001")),
            "Walk around the Petrovaradin fortress", 1200m, ownerId);

    [Fact]
    public void Owner_without_a_product_cannot_make_a_coupon()
    {
        var issuing = () => new CouponIssuance().Issue(
            WellKnownUsers.Explorer, null, [], 20, DateTime.UtcNow.AddDays(7));

        issuing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Coupon_tied_to_a_product_of_another_owner_is_refused()
    {
        var ownedProduct = CreateFortressTour(WellKnownUsers.Explorer);
        var foreignProduct = CreateFortressTour(WellKnownUsers.SecondExplorer);

        var issuing = () => new CouponIssuance().Issue(
            WellKnownUsers.Explorer, foreignProduct.Id, [ownedProduct], 20, DateTime.UtcNow.AddDays(7));

        issuing.Should().Throw<DomainException>();
    }
}
