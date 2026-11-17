using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Integration.ProductCatalog;

public class ProductCatalogCommandTests : BaseIntegrationTest
{
    public ProductCatalogCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tour_registered_for_the_first_time_joins_the_offer()
    {
        var registration = new ProductRegistrationDto(
            ProductType.Tour,
            Guid.Parse("30000000-0000-0000-0000-000000000009"),
            "Night walk through Novi Sad",
            1800m,
            DemoUsers.SecondExplorer);
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var productCountBefore = arrangeContext.Products.Count();

        await paymentApi.RegisterProductAsync(registration);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(productCountBefore + 1);
        var storedProduct = assertContext.Products.Single(product =>
            product.Reference.EntityId == registration.EntityId);
        storedProduct.Name.Should().Be(registration.Name);
        storedProduct.Price.Should().Be(registration.Price);
        storedProduct.OwnerId.Should().Be(registration.OwnerId);
    }

    [Fact]
    public async Task Tour_registered_again_keeps_one_product_and_takes_over_its_new_details()
    {
        var registration = new ProductRegistrationDto(
            ProductType.Tour,
            ProductSeed.DemoExplorersTour.Reference.EntityId,
            "Walk around the Petrovaradin fortress at dawn",
            1450m,
            DemoUsers.SecondExplorer);
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var productCountBefore = arrangeContext.Products.Count();

        await paymentApi.RegisterProductAsync(registration);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(productCountBefore);
        var storedProduct = assertContext.Products.Single(product =>
            product.Id == ProductSeed.DemoExplorersTour.Id);
        storedProduct.Name.Should().Be(registration.Name);
        storedProduct.Price.Should().Be(registration.Price);
        storedProduct.OwnerId.Should().Be(registration.OwnerId);
    }

    [Fact]
    public async Task Tour_registered_without_a_name_leaves_the_offer_as_it_was()
    {
        var registration = new ProductRegistrationDto(
            ProductType.Tour,
            ProductSeed.DemoExplorersTour.Reference.EntityId,
            "   ",
            1450m,
            DemoUsers.SecondExplorer);
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var productCountBefore = arrangeContext.Products.Count();

        var registering = async () => await paymentApi.RegisterProductAsync(registration);

        await registering.Should().ThrowAsync<DomainException>();
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(productCountBefore);
        var storedProduct = assertContext.Products.Single(product =>
            product.Id == ProductSeed.DemoExplorersTour.Id);
        storedProduct.Name.Should().Be(ProductSeed.DemoExplorersTour.Name);
        storedProduct.Price.Should().Be(ProductSeed.DemoExplorersTour.Price);
        storedProduct.OwnerId.Should().Be(ProductSeed.DemoExplorersTour.OwnerId);
    }

    [Fact]
    public async Task Content_of_a_kind_the_platform_does_not_sell_leaves_the_offer_as_it_was()
    {
        var registration = new ProductRegistrationDto(
            (ProductType)99,
            Guid.Parse("30000000-0000-0000-0000-000000000009"),
            "Night walk through Novi Sad",
            1800m,
            DemoUsers.SecondExplorer);
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var productCountBefore = arrangeContext.Products.Count();

        var registering = async () => await paymentApi.RegisterProductAsync(registration);

        await registering.Should().ThrowAsync<DomainException>();
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(productCountBefore);
    }
}
