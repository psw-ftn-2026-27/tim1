using Games.Domain.ChallengeCompletions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.ChallengeCompletions;

internal static class ChallengeCompletionConfiguration
{
    public static void ConfigureChallengeCompletions(this ModelBuilder modelBuilder)
    {
        ConfigureChallengeCompletion(modelBuilder.Entity<ChallengeCompletion>());
    }

    private static void ConfigureChallengeCompletion(EntityTypeBuilder<ChallengeCompletion> builder)
    {
        builder.ToTable("ChallengeCompletions");
        builder.Property(completion => completion.ChallengeType).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(completion => new { completion.PlayerId, completion.ChallengeId }).IsUnique();
    }
}
