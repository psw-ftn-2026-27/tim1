using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.ProductCatalog;
using Payment.Contracts;
using Payment.Tests.Integration.Seeds;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;
using ContractProductType = Payment.Contracts.ProductType;
using DomainProductType = Payment.Domain.Products.ProductType;

namespace Payment.Tests.Integration.ProductCatalog;

public class ProductCatalogQueryTests : BaseIntegrationTest
{
    public ProductCatalogQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_every_registered_product_with_the_name_of_its_owner()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/payment/products", JsonOptions);

        products!.Should().HaveCount(ProductSeed.All.Length);
        var tour = products.Single(product => product.Id == ProductSeed.DemoExplorersTour.Id);
        tour.Type.Should().Be(DomainProductType.Tour);
        tour.EntityId.Should().Be(ProductSeed.DemoExplorersTour.Reference.EntityId);
        tour.Name.Should().Be(ProductSeed.DemoExplorersTour.Name);
        tour.Price.Should().Be(ProductSeed.DemoExplorersTour.Price);
        tour.OwnerName.Should().NotBeNullOrWhiteSpace();
        tour.OwnerName.Should().NotBe(ProductCatalogQueries.UnknownOwnerName);
    }

    [Fact]
    public async Task Product_whose_owner_is_not_found_stays_in_the_offer_without_a_name()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/payment/products", JsonOptions);

        products!.Single(product => product.Id == ProductSeed.TourOfAForgottenOwner.Id)
            .OwnerName.Should().Be(ProductCatalogQueries.UnknownOwnerName);
    }

    [Fact]
    public async Task Product_the_explorer_already_bought_is_marked_as_purchased()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/payment/products", JsonOptions);

        var bought = products!.Single(product =>
            product.EntityId == PurchaseSeed.SecondExplorersRide.Reference.EntityId);
        bought.Purchased.Should().BeTrue();
        products!.Single(product => product.Id == ProductSeed.DemoExplorersTour.Id)
            .Purchased.Should().BeFalse();
    }

    [Fact]
    public async Task Product_waiting_in_the_explorers_cart_is_marked_as_in_the_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/payment/products", JsonOptions);

        products!.Single(product => product.Id == ProductSeed.DemoExplorersTour.Id)
            .InCart.Should().BeTrue();
    }

    [Fact]
    public async Task Explorer_without_a_cart_or_a_purchase_sees_no_marks()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/payment/products", JsonOptions);

        products!.Should().OnlyContain(product => !product.Purchased && !product.InCart);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_the_offer()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_the_offer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/products");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Search_without_filters_finds_the_whole_offer()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var products = await paymentApi.SearchProductsAsync(null, null, null);

        products.Select(product => product.Id)
            .Should().BeEquivalentTo(ProductSeed.All.Select(product => product.Id));
    }

    [Fact]
    public async Task Search_from_a_minimum_price_finds_the_products_that_cost_at_least_that_much()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var products = await paymentApi.SearchProductsAsync(ProductSeed.DemoExplorersTour.Price, null, null);

        var tour = products.Should().ContainSingle().Which;
        tour.Id.Should().Be(ProductSeed.DemoExplorersTour.Id);
        tour.Type.Should().Be(ContractProductType.Tour);
        tour.Name.Should().Be(ProductSeed.DemoExplorersTour.Name);
        tour.Price.Should().Be(ProductSeed.DemoExplorersTour.Price);
        tour.OwnerId.Should().Be(ProductSeed.DemoExplorersTour.OwnerId);
        tour.OwnerName.Should().NotBe(ProductCatalogQueries.UnknownOwnerName);
    }

    [Fact]
    public async Task Search_up_to_a_maximum_price_finds_the_products_that_cost_at_most_that_much()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var products = await paymentApi.SearchProductsAsync(null, ProductSeed.TourOfAForgottenOwner.Price, null);

        products.Should().ContainSingle()
            .Which.Id.Should().Be(ProductSeed.TourOfAForgottenOwner.Id);
    }

    [Fact]
    public async Task Search_in_a_price_range_finds_the_products_inside_it()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var products = await paymentApi.SearchProductsAsync(
            ProductSeed.TourOfAForgottenOwner.Price + 1,
            ProductSeed.DemoExplorersTour.Price,
            null);

        products.Should().ContainSingle()
            .Which.Id.Should().Be(ProductSeed.DemoExplorersTour.Id);
    }

    [Fact]
    public async Task Search_whose_minimum_price_is_above_its_maximum_finds_nothing()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var products = await paymentApi.SearchProductsAsync(
            ProductSeed.DemoExplorersTour.Price,
            ProductSeed.TourOfAForgottenOwner.Price,
            null);

        products.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_for_a_kind_of_content_finds_the_products_of_that_kind()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var products = await paymentApi.SearchProductsAsync(null, null, ContractProductType.Tour);

        products.Select(product => product.Id)
            .Should().BeEquivalentTo(ProductSeed.All.Select(product => product.Id));
    }

    [Fact]
    public async Task Search_for_a_kind_of_content_the_platform_does_not_sell_finds_nothing()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var searching = async () => await paymentApi.SearchProductsAsync(null, null, (ContractProductType)99);

        await searching.Should().ThrowAsync<DomainException>();
    }
}
