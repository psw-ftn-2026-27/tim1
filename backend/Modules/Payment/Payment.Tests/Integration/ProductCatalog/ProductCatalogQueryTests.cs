using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.ProductCatalog;
using Payment.Domain.Products;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

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
        tour.Type.Should().Be(ProductType.Tour);
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
}
