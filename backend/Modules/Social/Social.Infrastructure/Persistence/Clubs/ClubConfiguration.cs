using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal static class ClubConfiguration
{
    public static void ConfigureClubs(this ModelBuilder modelBuilder)
    {
        ConfigureClub(modelBuilder.Entity<Club>());
        ConfigureClubMember(modelBuilder.Entity<ClubMember>());
        ConfigureMembershipRequest(modelBuilder.Entity<MembershipRequest>());
    }

    private static void ConfigureClub(EntityTypeBuilder<Club> builder)
    {
        builder.Property(club => club.Id).ValueGeneratedNever();

        builder.HasMany(club => club.Members)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(club => club.MembershipRequests)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(club => club.Members).AutoInclude();
        builder.Navigation(club => club.MembershipRequests).AutoInclude();
    }

    private static void ConfigureClubMember(EntityTypeBuilder<ClubMember> builder)
    {
        builder.ToTable("ClubMembers");
        builder.Property(member => member.Id).ValueGeneratedNever();
        builder.Property(member => member.Role).HasConversion<string>().HasMaxLength(20);
    }

    private static void ConfigureMembershipRequest(EntityTypeBuilder<MembershipRequest> builder)
    {
        builder.ToTable("ClubMembershipRequests");
        builder.Property(request => request.Id).ValueGeneratedNever();
    }
}
