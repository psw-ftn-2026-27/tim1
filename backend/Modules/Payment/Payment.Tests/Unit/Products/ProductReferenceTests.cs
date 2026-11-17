using FluentAssertions;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.Products;

public class ProductReferenceTests
{
    [Fact]
    public void Product_reference_requires_the_entity_it_stands_for()
    {
        var reference = () => new ProductReference(ProductType.Tour, Guid.Empty);

        reference.Should().Throw<DomainException>();
    }
}
