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
        ConfigureClubInvitation(modelBuilder.Entity<ClubInvitation>());
    }

    private static void ConfigureClub(EntityTypeBuilder<Club> builder)
    {
        builder.Property(club => club.Id).ValueGeneratedNever();
        builder.Property(club => club.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(ClubStatus.Active);

        builder.HasMany(club => club.Members)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(club => club.MembershipRequests)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(club => club.Invitations)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(club => club.Members).AutoInclude();
        builder.Navigation(club => club.MembershipRequests).AutoInclude();
        builder.Navigation(club => club.Invitations).AutoInclude();
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

    private static void ConfigureClubInvitation(EntityTypeBuilder<ClubInvitation> builder)
    {
        builder.ToTable("ClubInvitations");
        builder.Property(invitation => invitation.Id).ValueGeneratedNever();
    }
}
