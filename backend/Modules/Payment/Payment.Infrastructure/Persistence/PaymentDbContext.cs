using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.TopUpRequests;
using Payment.Domain.Wallets;
using Payment.Domain.WithdrawalRequests;
using Payment.Infrastructure.Persistence.GiftCards;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.Purchases;
using Payment.Infrastructure.Persistence.ShoppingCarts;
using Payment.Infrastructure.Persistence.TopUpRequests;
using Payment.Infrastructure.Persistence.Wallets;
using Payment.Infrastructure.Persistence.WithdrawalRequests;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<TopUpRequest> TopUpRequests => Set<TopUpRequest>();
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();

    public DbSet<GiftCard> GiftCards => Set<GiftCard>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureProducts();
        builder.ConfigureShoppingCarts();
        builder.ConfigurePurchases();
        builder.ConfigureTopUpRequests();
        builder.ConfigureWithdrawalRequests();
        builder.ConfigureGiftCards();
    }
}
