using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubInviting;

public sealed class ClubInvitingQueries
{
    private readonly IClubReadRepository _clubReadRepository;
    private readonly IIdentityApi _identityApi;

    public ClubInvitingQueries(IClubReadRepository clubReadRepository, IIdentityApi identityApi)
    {
        _clubReadRepository = clubReadRepository;
        _identityApi = identityApi;
    }

    // Members and already invited users are not left out: inviting them is rejected by the
    // aggregate (invitations P5), so the rule stays in one place.
    public async Task<PageResult<UserToInviteDto>> SearchUsersToInviteAsync(string text, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _identityApi.SearchUsersAsync(text, page, pageSize);

        var items = users.Items
            .Select(user => new UserToInviteDto(user.Id, user.DisplayName))
            .ToList();

        return new PageResult<UserToInviteDto>(items, users.TotalCount);
    }

    // Invitations P1: only the club's owner gets its pending invitations; to anyone else the club does not exist.
    public async Task<List<SentInvitationDto>> GetSentInvitationsAsync(Guid clubId, Guid userId)
    {
        var invitations = await _clubReadRepository.GetSentInvitationsAsync(clubId, userId);
        if (invitations is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        var invitedIds = invitations.Select(invitation => invitation.UserId).Distinct().ToList();
        var invitedUsers = await _identityApi.GetUsersAsync(invitedIds);
        var displayNames = invitedUsers.ToDictionary(user => user.Id, user => user.DisplayName);

        return invitations
            .Select(invitation => invitation with { UserDisplayName = displayNames.GetValueOrDefault(invitation.UserId, "") })
            .ToList();
    }

    public Task<List<MyInvitationDto>> GetMyInvitationsAsync(Guid userId)
    {
        return _clubReadRepository.GetInvitationsOfUserAsync(userId);
    }
}
