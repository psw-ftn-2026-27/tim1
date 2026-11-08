using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.FollowRequests;

namespace Social.Infrastructure.Persistence.FollowRequests;

internal static class FollowRequestConfiguration
{
    public static void ConfigureFollowRequests(this ModelBuilder modelBuilder)
    {
        ConfigureFollowRequest(modelBuilder.Entity<FollowRequest>());
    }

    private static void ConfigureFollowRequest(EntityTypeBuilder<FollowRequest> builder)
    {
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(20);
    }
}
