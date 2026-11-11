using Exploration.Domain.TourPreferences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.TourPreferences;

internal static class TourPreferenceConfiguration
{
    public static void ConfigureTourPreferences(this ModelBuilder modelBuilder)
    {
        ConfigureTourPreference(modelBuilder.Entity<TourPreference>());
    }

    private static void ConfigureTourPreference(EntityTypeBuilder<TourPreference> builder)
    {
        builder.PrimitiveCollection(preference => preference.Difficulties)
            .ElementType(difficulty => difficulty.HasConversion<string>());
        builder.Property(preference => preference.Transport).HasConversion<string>().HasMaxLength(20);
    }
}
