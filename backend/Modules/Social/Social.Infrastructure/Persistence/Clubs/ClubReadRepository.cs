using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.ClubInviting;
using Social.Application.ClubMembership;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal sealed class ClubReadRepository : IClubReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ClubReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ClubDto?> GetByIdAsync(Guid id)
    {
        return ProjectToDtos(_dbContext.Clubs.Where(club => club.Id == id)).FirstOrDefaultAsync();
    }

    public Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId)
    {
        return ProjectToDtos(_dbContext.Clubs.Where(club => club.OwnerId == ownerId)).ToListAsync();
    }

    public async Task<PageResult<ClubDto>> GetAllAsync(int page, int pageSize)
    {
        var clubs = _dbContext.Clubs.OrderBy(club => club.Name);

        var totalCount = await clubs.CountAsync();
        var items = await ProjectToDtos(clubs)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<ClubDto>(items, totalCount);
    }

    public Task<List<ClubMemberDto>?> GetMembersAsync(Guid clubId)
    {
        return _dbContext.Clubs
            .Where(club => club.Id == clubId)
            .Select(club => club.Members
                .Select(member => new ClubMemberDto(member.UserId, "", member.Role))
                .ToList())
            .FirstOrDefaultAsync();
    }

    public Task<MyMembershipDto?> GetMembershipAsync(Guid clubId, Guid userId)
    {
        return _dbContext.Clubs
            .Where(club => club.Id == clubId)
            .Select(club => new MyMembershipDto(
                club.Members.Any(member => member.UserId == userId)
                    ? MembershipStatus.Member
                    : club.MembershipRequests.Any(request => request.UserId == userId)
                        ? MembershipStatus.PendingRequest
                        : MembershipStatus.NotMember,
                club.Members
                    .Where(member => member.UserId == userId)
                    .Select(member => (ClubRole?)member.Role)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync();
    }

    public Task<List<MembershipRequestDto>?> GetPendingRequestsAsync(Guid clubId, Guid resolverId)
    {
        return _dbContext.Clubs
            .Where(club => club.Id == clubId
                && club.Members.Any(member => member.UserId == resolverId
                    && (member.Role == ClubRole.Owner || member.Role == ClubRole.Moderator)))
            .Select(club => club.MembershipRequests
                .OrderBy(request => request.CreatedAt)
                .Select(request => new MembershipRequestDto(request.UserId, request.CreatedAt))
                .ToList())
            .FirstOrDefaultAsync();
    }

    public Task<List<SentInvitationDto>?> GetSentInvitationsAsync(Guid clubId, Guid ownerId)
    {
        return _dbContext.Clubs
            .Where(club => club.Id == clubId
                && club.Members.Any(member => member.UserId == ownerId && member.Role == ClubRole.Owner))
            .Select(club => club.Invitations
                .OrderBy(invitation => invitation.CreatedAt)
                .Select(invitation => new SentInvitationDto(invitation.UserId, "", invitation.CreatedAt))
                .ToList())
            .FirstOrDefaultAsync();
    }

    public Task<List<MyInvitationDto>> GetInvitationsOfUserAsync(Guid userId)
    {
        return _dbContext.Clubs
            .SelectMany(
                club => club.Invitations.Where(invitation => invitation.UserId == userId),
                (club, invitation) => new { club.Id, club.Name, invitation.CreatedAt })
            .OrderBy(row => row.CreatedAt)
            .Select(row => new MyInvitationDto(row.Id, row.Name, row.CreatedAt))
            .ToListAsync();
    }

    private static IQueryable<ClubDto> ProjectToDtos(IQueryable<Club> clubs)
    {
        return clubs.Select(club => new ClubDto(club.Id, club.OwnerId, club.Name, club.Description, club.Images));
    }
}
