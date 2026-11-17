using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubMembership;

public sealed class ClubMembershipService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubMembershipService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RequestMembershipAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.RequestMembership(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task WithdrawMembershipRequestAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.WithdrawMembershipRequest(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptMembershipRequestAsync(Guid clubId, Guid resolverId, Guid requesterId)
    {
        var club = await GetClubAsync(clubId);

        club.AcceptMembershipRequest(resolverId, requesterId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectMembershipRequestAsync(Guid clubId, Guid resolverId, Guid requesterId)
    {
        var club = await GetClubAsync(clubId);

        club.RejectMembershipRequest(resolverId, requesterId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetClubAsync(Guid clubId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }
}
