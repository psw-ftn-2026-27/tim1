using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Products;

/// <summary>
/// Something the platform sells. Payment keeps only what every product has, whatever module
/// the content comes from: which entity it stands for, its name, its price, and who earns
/// from it. A product knows nothing about purchases, so changing its details changes the
/// current offer alone.
/// </summary>
public sealed class Product : AggregateRoot
{
    public ProductReference Reference { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public Guid OwnerId { get; private set; }

    private Product()
    {
        Reference = null!;
        Name = null!;
    }

    public Product(ProductReference reference, string name, decimal price, Guid ownerId) : base(Guid.NewGuid())
    {
        Validate(name, price, ownerId);

        Reference = reference;
        Name = name;
        Price = price;
        OwnerId = ownerId;
    }

    public void ChangeDetails(string name, decimal price, Guid ownerId)
    {
        Validate(name, price, ownerId);

        Name = name;
        Price = price;
        OwnerId = ownerId;
    }

    private static void Validate(string name, decimal price, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A product requires a name.");
        }

        if (price < 0)
        {
            throw new DomainException("A product cannot have a negative price.");
        }

        if (ownerId == Guid.Empty)
        {
            throw new DomainException("A product requires an owner.");
        }
    }
}
