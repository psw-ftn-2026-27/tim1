using Exploration.Domain.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.Images;

internal static class ImageConfiguration
{
    public static void ConfigureImages(this ModelBuilder modelBuilder)
    {
        ConfigureImage(modelBuilder.Entity<Image>());
    }

    private static void ConfigureImage(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("Images");
        builder.Property(image => image.Id).ValueGeneratedNever();
        builder.Property(image => image.StoredFileName).HasMaxLength(100);
        builder.Property(image => image.Status).HasConversion<string>().HasMaxLength(20);
    }
}
