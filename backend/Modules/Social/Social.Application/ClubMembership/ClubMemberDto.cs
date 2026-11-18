using Social.Domain.Clubs;

namespace Social.Application.ClubMembership;

public sealed record ClubMemberDto(Guid UserId, ClubRole Role);
