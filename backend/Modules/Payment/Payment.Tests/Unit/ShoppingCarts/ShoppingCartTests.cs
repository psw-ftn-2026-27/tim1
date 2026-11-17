using FluentAssertions;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public class ShoppingCartTests
{
    private static readonly ProductReference FortressTour =
        new(ProductType.Tour, Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private static readonly ProductReference MountainRide =
        new(ProductType.Tour, Guid.Parse("40000000-0000-0000-0000-000000000002"));

    private static ShoppingCart CreateCartWithTheFortressTour()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);
        cart.AddItem(FortressTour, "Walk around the Petrovaradin fortress", 1200m);
        return cart;
    }

    private static ShoppingCart CreateCartWithBothTours()
    {
        var cart = CreateCartWithTheFortressTour();
        cart.AddItem(MountainRide, "Ride across Fruska gora", 900m);
        return cart;
    }

    [Fact]
    public void Product_already_in_the_cart_is_not_added_a_second_time()
    {
        var cart = CreateCartWithBothTours();

        var adding = () => cart.AddItem(FortressTour, "Walk around the Petrovaradin fortress at dawn", 1500m);

        adding.Should().Throw<DomainException>();
        cart.Items.Should().HaveCount(2);
        cart.Total().Should().Be(2100m);
    }

    [Fact]
    public void Cart_without_items_comes_to_nothing()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);

        var total = cart.Total();

        total.Should().Be(0);
    }

    [Fact]
    public void Added_product_raises_the_total_by_its_price()
    {
        var cart = CreateCartWithTheFortressTour();

        cart.AddItem(MountainRide, "Ride across Fruska gora", 900m);

        cart.Total().Should().Be(2100m);
    }

    [Fact]
    public void Removed_item_lowers_the_total_by_its_price()
    {
        var cart = CreateCartWithBothTours();

        cart.RemoveItem(MountainRide);

        cart.Items.Should().ContainSingle(item => item.Reference == FortressTour);
        cart.Total().Should().Be(1200m);
    }

    [Fact]
    public void Item_the_cart_does_not_hold_cannot_be_removed()
    {
        var cart = CreateCartWithTheFortressTour();

        var removing = () => cart.RemoveItem(MountainRide);

        removing.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle(item => item.Reference == FortressTour);
    }
}
