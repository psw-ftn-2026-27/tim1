using Microsoft.EntityFrameworkCore;
using Payment.Application.Purchases;
using Payment.Domain.Products;
using Payment.Domain.Purchases;

namespace Payment.Infrastructure.Persistence.Purchases;

internal sealed class PurchaseRepository : IPurchaseRepository
{
    private readonly PaymentDbContext _dbContext;

    public PurchaseRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// What the buyer has already bought among the given references. The database narrows the
    /// rows down by entity ID, and the pair of kind and ID decides which of them count.
    /// </summary>
    public async Task<List<Purchase>> GetOfBuyerAsync(Guid buyerId, IReadOnlyList<ProductReference> references)
    {
        var entityIds = references.Select(reference => reference.EntityId).ToList();

        var purchases = await _dbContext.Purchases
            .Where(purchase => purchase.BuyerId == buyerId && entityIds.Contains(purchase.Reference.EntityId))
            .ToListAsync();

        return [.. purchases.Where(purchase => references.Contains(purchase.Reference))];
    }

    public void Add(Purchase purchase)
    {
        _dbContext.Purchases.Add(purchase);
    }
}
