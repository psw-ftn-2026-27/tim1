using Social.Domain.Clubs;

namespace Social.Application.ClubMembership;

// Role is null when the user is not a member of the club.
public sealed record MyMembershipDto(MembershipStatus Status, ClubRole? Role);
