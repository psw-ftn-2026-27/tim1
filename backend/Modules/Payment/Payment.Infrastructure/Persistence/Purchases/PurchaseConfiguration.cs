using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Purchases;

namespace Payment.Infrastructure.Persistence.Purchases;

internal static class PurchaseConfiguration
{
    public static void ConfigurePurchases(this ModelBuilder modelBuilder)
    {
        ConfigurePurchase(modelBuilder.Entity<Purchase>());
    }

    private static void ConfigurePurchase(EntityTypeBuilder<Purchase> builder)
    {
        builder.OwnsOne(purchase => purchase.Reference, reference =>
        {
            reference.Property(entity => entity.EntityType).HasConversion<string>().HasMaxLength(20);
        });
        builder.Navigation(purchase => purchase.Reference).IsRequired();
    }
}
