using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.TopUpRequests;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.TopUpRequests;
using Payment.Infrastructure.Persistence.Wallets;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<TopUpRequest> TopUpRequests => Set<TopUpRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureTopUpRequests();
    }
}
