using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.ShoppingCarts;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.ShoppingCarts;

public class ShoppingCartQueryTests : BaseIntegrationTest
{
    public ShoppingCartQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Customer_sees_every_item_of_their_cart_and_the_total_they_come_to()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/cart", JsonOptions);

        cart!.Items.Should().HaveCount(ShoppingCartSeed.ExplorersCart.Items.Count);
        cart.Items.Select(item => item.EntityId).Should().BeEquivalentTo(
            ShoppingCartSeed.ExplorersCart.Items.Select(item => item.Reference.EntityId));
        cart.Total.Should().Be(ShoppingCartSeed.ExplorersCart.Total());
    }

    [Fact]
    public async Task Item_keeps_the_name_and_the_price_it_had_when_it_was_added()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seededItem = ShoppingCartSeed.ExplorersCart.Items.Single(item =>
            item.Reference == ProductSeed.DemoExplorersTour.Reference);

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/cart", JsonOptions);

        var item = cart!.Items.Single(cartItem => cartItem.EntityId == seededItem.Reference.EntityId);
        item.Name.Should().Be(seededItem.Name);
        item.Price.Should().Be(seededItem.Price);
        item.Price.Should().NotBe(ProductSeed.DemoExplorersTour.Price);
    }

    [Fact]
    public async Task Cart_of_a_customer_who_has_none_is_empty_and_no_cart_is_opened_for_them()
    {
        var newCustomer = Guid.NewGuid();
        var client = Factory.CreateClientFor(newCustomer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.ShoppingCarts.Count();

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/cart", JsonOptions);

        cart!.Items.Should().BeEmpty();
        cart.Total.Should().Be(0);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Count().Should().Be(cartCountBefore);
        assertContext.ShoppingCarts.Should().NotContain(storedCart => storedCart.OwnerId == newCustomer);
    }

    [Fact]
    public async Task Customer_sees_only_the_items_of_their_own_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/cart", JsonOptions);

        cart!.Items.Select(item => item.EntityId).Should().BeEquivalentTo(
            ShoppingCartSeed.SecondExplorersCart.Items.Select(item => item.Reference.EntityId));
        cart.Total.Should().Be(ShoppingCartSeed.SecondExplorersCart.Total());
    }

    [Fact]
    public async Task Customer_counts_only_the_items_of_their_own_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var itemCount = await client.GetFromJsonAsync<int>("/api/payment/cart/item-count", JsonOptions);

        itemCount.Should().Be(ShoppingCartSeed.SecondExplorersCart.Items.Count);
    }

    [Fact]
    public async Task Customer_without_a_cart_counts_no_items()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var itemCount = await client.GetFromJsonAsync<int>("/api/payment/cart/item-count", JsonOptions);

        itemCount.Should().Be(0);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/cart");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/cart");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
