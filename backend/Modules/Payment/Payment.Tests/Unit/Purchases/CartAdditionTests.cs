using FluentAssertions;
using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Purchases;

public class CartAdditionTests
{
    private static readonly ProductReference FortressTour =
        new(ProductType.Tour, Guid.Parse("50000000-0000-0000-0000-000000000001"));

    private static Product CreateFortressTour() =>
        new(FortressTour, "Walk around the Petrovaradin fortress", 1200m, WellKnownUsers.SecondExplorer);

    [Fact]
    public void Product_the_customer_does_not_own_joins_their_cart()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);

        new CartAddition().Add(cart, CreateFortressTour(), []);

        cart.Items.Should().ContainSingle(item => item.Reference == FortressTour);
    }

    [Fact]
    public void Product_the_customer_already_bought_does_not_join_their_cart()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);
        var owned = new Purchase(WellKnownUsers.Explorer, FortressTour, 1200m);

        var adding = () => new CartAddition().Add(cart, CreateFortressTour(), [owned]);

        adding.Should().Throw<DomainException>();
        cart.Items.Should().BeEmpty();
    }
}
