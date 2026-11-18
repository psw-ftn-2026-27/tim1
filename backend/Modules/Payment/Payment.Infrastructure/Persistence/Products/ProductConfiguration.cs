using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Products;

namespace Payment.Infrastructure.Persistence.Products;

internal static class ProductConfiguration
{
    public static void ConfigureProducts(this ModelBuilder modelBuilder)
    {
        ConfigureProduct(modelBuilder.Entity<Product>());
    }

    private static void ConfigureProduct(EntityTypeBuilder<Product> builder)
    {
        builder.Property(product => product.Name).HasMaxLength(200);

        builder.OwnsOne(product => product.Reference, reference =>
        {
            reference.Property(entity => entity.EntityType).HasConversion<string>().HasMaxLength(20);
            reference.HasIndex(entity => new { entity.EntityType, entity.EntityId }).IsUnique();
        });
        builder.Navigation(product => product.Reference).IsRequired();
    }
}
