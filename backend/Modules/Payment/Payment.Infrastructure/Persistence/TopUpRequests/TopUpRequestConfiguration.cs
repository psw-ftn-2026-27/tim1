using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.TopUpRequests;

namespace Payment.Infrastructure.Persistence.TopUpRequests;

internal static class TopUpRequestConfiguration
{
    public static void ConfigureTopUpRequests(this ModelBuilder modelBuilder)
    {
        ConfigureTopUpRequest(modelBuilder.Entity<TopUpRequest>());
    }

    private static void ConfigureTopUpRequest(EntityTypeBuilder<TopUpRequest> builder)
    {
        builder.Property(topUpRequest => topUpRequest.Status).HasConversion<string>().HasMaxLength(20);
    }
}
