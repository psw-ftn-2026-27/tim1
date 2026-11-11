using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal static class ClubConfiguration
{
    public static void ConfigureClubs(this ModelBuilder modelBuilder)
    {
        ConfigureClub(modelBuilder.Entity<Club>());
    }

    private static void ConfigureClub(EntityTypeBuilder<Club> builder)
    {
        builder.Property(club => club.Id).ValueGeneratedNever();
    }
}
