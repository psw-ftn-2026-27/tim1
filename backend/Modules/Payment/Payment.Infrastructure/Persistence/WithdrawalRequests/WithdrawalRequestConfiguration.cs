using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.WithdrawalRequests;

namespace Payment.Infrastructure.Persistence.WithdrawalRequests;

internal static class WithdrawalRequestConfiguration
{
    public static void ConfigureWithdrawalRequests(this ModelBuilder modelBuilder)
    {
        ConfigureWithdrawalRequest(modelBuilder.Entity<WithdrawalRequest>());
    }

    private static void ConfigureWithdrawalRequest(EntityTypeBuilder<WithdrawalRequest> builder)
    {
        builder.Property(withdrawalRequest => withdrawalRequest.Status).HasConversion<string>().HasMaxLength(20);
    }
}
