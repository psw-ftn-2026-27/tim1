using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubInviting;

public sealed class ClubInvitingService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubInvitingService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task InviteMemberAsync(Guid clubId, Guid inviterId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.InviteMember(inviterId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptInvitationAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.AcceptInvitation(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeclineInvitationAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.DeclineInvitation(userId);
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
