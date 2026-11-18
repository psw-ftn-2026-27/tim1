using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;
using Payment.Application.Purchases;

namespace Payment.Infrastructure.Persistence.Purchases;

internal sealed class PurchaseReadRepository : IPurchaseReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public PurchaseReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
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
