using Games.Domain.LocationChallengeAttempts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallengeAttempts;

internal static class LocationChallengeAttemptConfiguration
{
    public static void ConfigureLocationChallengeAttempts(this ModelBuilder modelBuilder)
    {
        ConfigureLocationChallengeAttempt(modelBuilder.Entity<LocationChallengeAttempt>());
    }

    // One started attempt per explorer and challenge (P6) is also kept by a unique index.
    private static void ConfigureLocationChallengeAttempt(EntityTypeBuilder<LocationChallengeAttempt> builder)
    {
        builder.ComplexProperty(attempt => attempt.Circle, circle => circle.ComplexProperty(c => c.Center));
        builder.HasIndex(attempt => new { attempt.ChallengeId, attempt.UserId }).IsUnique();
    }
}
