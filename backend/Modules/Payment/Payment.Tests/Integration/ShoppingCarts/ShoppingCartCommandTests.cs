using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Products;
using Payment.Domain.Products;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.ShoppingCarts;

public class ShoppingCartCommandTests : BaseIntegrationTest
{
    public ShoppingCartCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Customer_adds_a_registered_product_to_their_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var item = new ProductReferenceDto(ProductType.Tour, ProductSeed.DemoExplorersTour.Reference.EntityId);

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", item);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedCart = assertContext.ShoppingCarts.Single(cart =>
            cart.OwnerId == WellKnownUsers.SecondExplorer);
        storedCart.Items.Should().HaveCount(ShoppingCartSeed.SecondExplorersCart.Items.Count + 1);
        var addedItem = storedCart.Items.Single(storedItem =>
            storedItem.Reference == ProductSeed.DemoExplorersTour.Reference);
        addedItem.Name.Should().Be(ProductSeed.DemoExplorersTour.Name);
        addedItem.Price.Should().Be(ProductSeed.DemoExplorersTour.Price);
    }

    [Fact]
    public async Task First_addition_opens_a_cart_for_a_customer_who_had_none()
    {
        var newCustomer = Guid.NewGuid();
        var client = Factory.CreateClientFor(newCustomer, "explorer");
        var item = new ProductReferenceDto(ProductType.Tour, ProductSeed.DemoExplorersTour.Reference.EntityId);

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", item);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedCart = assertContext.ShoppingCarts.Single(cart => cart.OwnerId == newCustomer);
        storedCart.Items.Should().ContainSingle(storedItem =>
            storedItem.Reference == ProductSeed.DemoExplorersTour.Reference);
    }

    [Fact]
    public async Task Product_that_is_not_registered_does_not_reach_the_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var item = new ProductReferenceDto(ProductType.Tour, Guid.Parse("30000000-0000-0000-0000-000000000009"));
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.ShoppingCarts.Count();

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", item);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Count().Should().Be(cartCountBefore);
        var storedCart = assertContext.ShoppingCarts.Single(cart =>
            cart.OwnerId == WellKnownUsers.SecondExplorer);
        storedCart.Items.Should().HaveCount(ShoppingCartSeed.SecondExplorersCart.Items.Count);
    }

    [Fact]
    public async Task Product_already_in_the_cart_is_not_added_a_second_time()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var item = new ProductReferenceDto(ProductType.Tour, ProductSeed.TourOfAForgottenOwner.Reference.EntityId);

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", item);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedCart = assertContext.ShoppingCarts.Single(cart =>
            cart.OwnerId == WellKnownUsers.SecondExplorer);
        storedCart.Items.Should().HaveCount(ShoppingCartSeed.SecondExplorersCart.Items.Count);
    }

    [Fact]
    public async Task Removed_item_leaves_the_cart_and_takes_its_price_out_of_the_total()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var removedItem = ProductSeed.TourOfAForgottenOwner.Reference;

        var response = await client.DeleteAsync(
            $"/api/payment/cart/items/{removedItem.EntityType}/{removedItem.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedCart = assertContext.ShoppingCarts.Single(cart => cart.OwnerId == WellKnownUsers.Explorer);
        storedCart.Items.Should().HaveCount(ShoppingCartSeed.ExplorersCart.Items.Count - 1);
        storedCart.Items.Should().NotContain(storedItem => storedItem.Reference == removedItem);
        storedCart.Total().Should().Be(ShoppingCartSeed.ExplorersCart.Total() - ProductSeed.TourOfAForgottenOwner.Price);
    }

    [Fact]
    public async Task Product_the_customer_already_bought_does_not_reach_their_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var item = new ProductReferenceDto(
            ProductType.Tour, PurchaseSeed.SecondExplorersRide.Reference.EntityId);
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var itemCountBefore = arrangeContext.ShoppingCarts
            .Single(cart => cart.OwnerId == WellKnownUsers.SecondExplorer).Items.Count;

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", item);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts
            .Single(cart => cart.OwnerId == WellKnownUsers.SecondExplorer)
            .Items.Count.Should().Be(itemCountBefore);
    }

    [Fact]
    public async Task Customer_who_has_no_cart_cannot_remove_an_item()
    {
        var newCustomer = Guid.NewGuid();
        var client = Factory.CreateClientFor(newCustomer, "explorer");
        var removedItem = ProductSeed.TourOfAForgottenOwner.Reference;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.ShoppingCarts.Count();

        var response = await client.DeleteAsync(
            $"/api/payment/cart/items/{removedItem.EntityType}/{removedItem.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Count().Should().Be(cartCountBefore);
        assertContext.ShoppingCarts.Should().NotContain(cart => cart.OwnerId == newCustomer);
    }

    [Fact]
    public async Task Product_the_cart_does_not_hold_cannot_be_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var removedItem = ProductSeed.DemoExplorersTour.Reference;

        var response = await client.DeleteAsync(
            $"/api/payment/cart/items/{removedItem.EntityType}/{removedItem.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedCart = assertContext.ShoppingCarts.Single(cart =>
            cart.OwnerId == WellKnownUsers.SecondExplorer);
        storedCart.Items.Should().HaveCount(ShoppingCartSeed.SecondExplorersCart.Items.Count);
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_product_to_a_cart()
    {
        var client = Factory.CreateClient();
        var item = new ProductReferenceDto(ProductType.Tour, ProductSeed.DemoExplorersTour.Reference.EntityId);

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", item);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_product_to_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var item = new ProductReferenceDto(ProductType.Tour, ProductSeed.DemoExplorersTour.Reference.EntityId);

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", item);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_an_item_from_a_cart()
    {
        var client = Factory.CreateClient();
        var removedItem = ProductSeed.TourOfAForgottenOwner.Reference;

        var response = await client.DeleteAsync(
            $"/api/payment/cart/items/{removedItem.EntityType}/{removedItem.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_an_item_from_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var removedItem = ProductSeed.TourOfAForgottenOwner.Reference;

        var response = await client.DeleteAsync(
            $"/api/payment/cart/items/{removedItem.EntityType}/{removedItem.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
