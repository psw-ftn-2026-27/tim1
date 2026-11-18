using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal static class LocationChallengeConfiguration
{
    public static void ConfigureLocationChallenges(this ModelBuilder modelBuilder)
    {
        ConfigureLocationChallenge(modelBuilder.Entity<LocationChallenge>());
        ConfigureHiddenPoint(modelBuilder.Entity<HiddenPoint>());
    }

    private static void ConfigureLocationChallenge(EntityTypeBuilder<LocationChallenge> builder)
    {
        builder.Property(challenge => challenge.Status).HasConversion<string>().HasMaxLength(20);
        builder.ComplexProperty(challenge => challenge.StartLocation);
        builder.HasMany(challenge => challenge.HiddenPoints)
            .WithOne()
            .HasForeignKey("LocationChallengeId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(challenge => challenge.HiddenPoints).AutoInclude();
    }

    // A changed draft gets new hidden points with IDs from their constructor, so EF must
    // treat a point with an ID as new rather than as an existing row.
    private static void ConfigureHiddenPoint(EntityTypeBuilder<HiddenPoint> builder)
    {
        builder.ToTable("HiddenPoints");
        builder.Property(point => point.Id).ValueGeneratedNever();
        builder.ComplexProperty(point => point.Location);
    }
}
