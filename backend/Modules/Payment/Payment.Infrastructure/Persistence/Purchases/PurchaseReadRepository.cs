using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;
using Payment.Application.Purchases;
using Payment.Domain.Products;

namespace Payment.Infrastructure.Persistence.Purchases;

internal sealed class PurchaseReadRepository : IPurchaseReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public PurchaseReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasPurchasedAsync(Guid buyerId, ProductReference reference)
    {
        return _dbContext.Purchases.AnyAsync(purchase =>
            purchase.BuyerId == buyerId
            && purchase.Reference.EntityType == reference.EntityType
            && purchase.Reference.EntityId == reference.EntityId);
    }

    public Task<List<ProductReferenceDto>> GetPurchasedReferencesAsync(Guid buyerId)
    {
        return _dbContext.Purchases
            .Where(purchase => purchase.BuyerId == buyerId)
            .Select(purchase => new ProductReferenceDto(
                purchase.Reference.EntityType, purchase.Reference.EntityId))
            .ToListAsync();
    }
}
