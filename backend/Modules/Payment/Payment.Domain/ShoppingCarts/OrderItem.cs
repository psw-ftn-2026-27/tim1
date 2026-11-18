using Payment.Domain.Products;

namespace Payment.Domain.ShoppingCarts;

/// <summary>
/// One product a customer has put in their cart. The item copies the name and the price the
/// product had at that moment, so a later change of the product leaves the cart alone.
/// </summary>
public sealed record OrderItem
{
    public ProductReference Reference { get; private init; }
    public string Name { get; private init; }
    public decimal Price { get; private init; }

    private OrderItem()
    {
        Reference = null!;
        Name = null!;
    }

    public OrderItem(ProductReference reference, string name, decimal price)
    {
        Reference = reference;
        Name = name;
        Price = price;
    }
}
