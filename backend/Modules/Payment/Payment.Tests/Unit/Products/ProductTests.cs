using FluentAssertions;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Products;

public class ProductTests
{
    private static ProductReference CreateReference() => new(ProductType.Tour, Guid.NewGuid());

    private static Product CreateProduct() =>
        new(CreateReference(), "Walk around the Petrovaradin fortress", 1200m, WellKnownUsers.Explorer);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Registered_product_requires_a_name(string name)
    {
        var registration = () => new Product(CreateReference(), name, 1200m, WellKnownUsers.Explorer);

        registration.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1200)]
    public void Registered_product_cannot_cost_less_than_nothing(decimal price)
    {
        var registration = () => new Product(
            CreateReference(), "Walk around the Petrovaradin fortress", price, WellKnownUsers.Explorer);

        registration.Should().Throw<DomainException>();
    }

    [Fact]
    public void Registered_product_requires_an_owner()
    {
        var registration = () => new Product(
            CreateReference(), "Walk around the Petrovaradin fortress", 1200m, Guid.Empty);

        registration.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Product_cannot_lose_its_name_by_being_registered_again(string name)
    {
        var product = CreateProduct();

        var change = () => product.ChangeDetails(name, 1500m, WellKnownUsers.SecondExplorer);

        change.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1200)]
    public void Product_cannot_come_to_cost_less_than_nothing_by_being_registered_again(decimal price)
    {
        var product = CreateProduct();

        var change = () => product.ChangeDetails("Ride across Fruska gora", price, WellKnownUsers.SecondExplorer);

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void Product_cannot_lose_its_owner_by_being_registered_again()
    {
        var product = CreateProduct();

        var change = () => product.ChangeDetails("Ride across Fruska gora", 1500m, Guid.Empty);

        change.Should().Throw<DomainException>();
    }
}
