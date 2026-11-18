using Exploration.Domain.UserPositions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.UserPositions;

internal static class UserPositionConfiguration
{
    public static void ConfigureUserPositions(this ModelBuilder modelBuilder)
    {
        ConfigureUserPosition(modelBuilder.Entity<UserPosition>());
    }

    private static void ConfigureUserPosition(EntityTypeBuilder<UserPosition> builder)
    {
        builder.HasIndex(position => position.UserId).IsUnique();
    }
}
