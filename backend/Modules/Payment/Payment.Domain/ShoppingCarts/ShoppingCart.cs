using Payment.Domain.Products;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

/// <summary>
/// What one customer has chosen to buy, with the total it comes to. The cart holds a snapshot
/// of every product it carries and never touches a wallet, so choosing costs nothing until the
/// customer pays.
/// </summary>
public sealed class ShoppingCart : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    public Guid OwnerId { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items;

    private ShoppingCart() { }

    public ShoppingCart(Guid ownerId) : base(Guid.NewGuid())
    {
        OwnerId = ownerId;
    }

    public void AddItem(ProductReference reference, string name, decimal price)
    {
        if (_items.Any(item => item.Reference == reference))
        {
            throw new DomainException("The product is already in the cart.");
        }

        _items.Add(new OrderItem(reference, name, price));
    }

    public void RemoveItem(ProductReference reference)
    {
        var item = _items.FirstOrDefault(item => item.Reference == reference);
        if (item is null)
        {
            throw new DomainException("The cart does not hold that product.");
        }

        _items.Remove(item);
    }

    /// <summary>Drops every item, which is what a paid cart is left as.</summary>
    public void Clear()
    {
        _items.Clear();
    }

    public decimal Total()
    {
        return _items.Sum(item => item.Price);
    }
}
