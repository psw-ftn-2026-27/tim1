using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal static class ShoppingCartConfiguration
{
    public static void ConfigureShoppingCarts(this ModelBuilder modelBuilder)
    {
        ConfigureShoppingCart(modelBuilder.Entity<ShoppingCart>());
    }

    private static void ConfigureShoppingCart(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.HasIndex(cart => cart.OwnerId).IsUnique();

        builder.OwnsMany(cart => cart.Items, items =>
        {
            items.ToTable("OrderItems");
            items.Property(item => item.Name).HasMaxLength(200);

            items.OwnsOne(item => item.Reference, reference =>
            {
                reference.Property(entity => entity.EntityType).HasConversion<string>().HasMaxLength(20);
            });
            items.Navigation(item => item.Reference).IsRequired();
        });
    }
}
