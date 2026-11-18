using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<string> Images { get; private set; }
    private readonly List<ClubMember> _members = [];
    public IReadOnlyList<ClubMember> Members => _members;
    private readonly List<MembershipRequest> _membershipRequests = [];
    public IReadOnlyList<MembershipRequest> MembershipRequests => _membershipRequests;
    private readonly List<ClubInvitation> _invitations = [];
    public IReadOnlyList<ClubInvitation> Invitations => _invitations;

    private Club()
    {
        Name = null!;
        Description = null!;
        Images = null!;
    }

    public Club(Guid ownerId, string name, string description, List<string>? images)
        : base(Guid.NewGuid())
    {
        OwnerId = ownerId;
        Name = null!;
        Description = null!;
        Images = null!;
        SetDetails(name, description, images);
        _members.Add(new ClubMember(ownerId, ClubRole.Owner));
    }

    public void Update(string name, string description, List<string>? images)
    {
        SetDetails(name, description, images);
    }

    // P2, P3; invitations P6. The "only an active club" part of P2 belongs here once the club gets a status.
    public void RequestMembership(Guid userId)
    {
        if (IsMember(userId))
        {
            throw new DomainException("A club member cannot request membership in the same club.");
        }
        if (HasPendingRequest(userId))
        {
            throw new DomainException("A membership request for this club is already pending.");
        }
        if (HasPendingInvitation(userId))
        {
            throw new DomainException("A user with a pending invitation to this club cannot request membership in it.");
        }

        _membershipRequests.Add(new MembershipRequest(userId));
    }

    // P4, P7: the request is found by its sender, so nobody can withdraw another user's request.
    public void WithdrawMembershipRequest(Guid userId)
    {
        _membershipRequests.Remove(GetMembershipRequest(userId));
    }

    // P5, P6, P7
    public void AcceptMembershipRequest(Guid resolverId, Guid requesterId)
    {
        EnsureCanResolveRequests(resolverId);
        var request = GetMembershipRequest(requesterId);

        _membershipRequests.Remove(request);
        _members.Add(new ClubMember(requesterId, ClubRole.Member));
    }

    // P5, P7
    public void RejectMembershipRequest(Guid resolverId, Guid requesterId)
    {
        EnsureCanResolveRequests(resolverId);

        _membershipRequests.Remove(GetMembershipRequest(requesterId));
    }

    // Invitations P1, P5. The "only an active club" part of P1 belongs here once the club gets a status.
    public void InviteMember(Guid inviterId, Guid userId)
    {
        EnsureCanInvite(inviterId);
        if (IsMember(userId))
        {
            throw new DomainException("A club member cannot be invited to the same club.");
        }
        if (HasPendingInvitation(userId))
        {
            throw new DomainException("An invitation to this club is already pending for the user.");
        }
        if (HasPendingRequest(userId))
        {
            throw new DomainException("The user already has a pending membership request for this club.");
        }

        _invitations.Add(new ClubInvitation(userId));
    }

    // Invitations P2, P3, P4: the invitation is found by the invited user, so nobody else can accept it.
    public void AcceptInvitation(Guid userId)
    {
        var invitation = GetInvitation(userId);

        _invitations.Remove(invitation);
        _members.Add(new ClubMember(userId, ClubRole.Member));
    }

    // Invitations P2, P4
    public void DeclineInvitation(Guid userId)
    {
        _invitations.Remove(GetInvitation(userId));
    }

    // P1, P2. The "only an active club" part of P1 belongs here once the club gets a status.
    public void PromoteToModerator(Guid ownerId, Guid memberId)
    {
        EnsureIsOwner(ownerId);
        var member = _members.FirstOrDefault(member => member.UserId == memberId);
        if (member is null || member.Role != ClubRole.Member)
        {
            throw new DomainException("Only an ordinary club member can be promoted to moderator.");
        }

        member.ChangeRole(ClubRole.Moderator);
    }

    // P1, P3, P4, P5, P6. The "only an active club" part of P1 belongs here once the club gets a status.
    public void TransferOwnership(Guid ownerId, Guid memberId)
    {
        var owner = EnsureIsOwner(ownerId);
        var newOwner = _members.FirstOrDefault(member => member.UserId == memberId);
        if (newOwner is null || memberId == ownerId)
        {
            throw new DomainException("Ownership can only be transferred to another club member.");
        }

        owner.ChangeRole(ClubRole.Member);
        newOwner.ChangeRole(ClubRole.Owner);
        OwnerId = memberId;
    }

    // Removal P1, P2, P3. No "only an active club" check once the club gets a status, because P4 allows removal in a closed club.
    public void RemoveMember(Guid removerId, Guid memberId)
    {
        var remover = _members.FirstOrDefault(member => member.UserId == removerId
            && (member.Role == ClubRole.Owner || member.Role == ClubRole.Moderator));
        if (remover is null)
        {
            throw new DomainException("Only the club owner or a moderator can remove members.");
        }
        var member = _members.FirstOrDefault(member => member.UserId == memberId);
        if (member is null)
        {
            throw new NotFoundException($"Member {memberId} of the club does not exist.");
        }
        if (remover.Role == ClubRole.Owner && memberId == removerId)
        {
            throw new DomainException("The club owner cannot remove themselves from the club.");
        }
        if (remover.Role == ClubRole.Moderator && member.Role != ClubRole.Member)
        {
            throw new DomainException("A moderator can remove only ordinary members.");
        }

        _members.Remove(member);
    }

    private bool IsMember(Guid userId)
    {
        return _members.Any(member => member.UserId == userId);
    }

    private bool HasPendingRequest(Guid userId)
    {
        return _membershipRequests.Any(request => request.UserId == userId);
    }

    private bool HasPendingInvitation(Guid userId)
    {
        return _invitations.Any(invitation => invitation.UserId == userId);
    }

    private void EnsureCanInvite(Guid inviterId)
    {
        var isOwner = _members.Any(member => member.UserId == inviterId && member.Role == ClubRole.Owner);
        if (!isOwner)
        {
            throw new DomainException("Only the club owner can invite users to the club.");
        }
    }

    private ClubInvitation GetInvitation(Guid userId)
    {
        var invitation = _invitations.FirstOrDefault(invitation => invitation.UserId == userId);
        if (invitation is null)
        {
            throw new NotFoundException($"Invitation of user {userId} does not exist.");
        }
        return invitation;
    }

    private void EnsureCanResolveRequests(Guid resolverId)
    {
        var canResolve = _members.Any(member => member.UserId == resolverId
            && (member.Role == ClubRole.Owner || member.Role == ClubRole.Moderator));
        if (!canResolve)
        {
            throw new DomainException("Only the club owner or a moderator can resolve membership requests.");
        }
    }

    private ClubMember EnsureIsOwner(Guid ownerId)
    {
        var owner = _members.FirstOrDefault(member => member.UserId == ownerId && member.Role == ClubRole.Owner);
        if (owner is null)
        {
            throw new DomainException("Only the club owner can change member roles.");
        }
        return owner;
    }

    private MembershipRequest GetMembershipRequest(Guid userId)
    {
        var request = _membershipRequests.FirstOrDefault(request => request.UserId == userId);
        if (request is null)
        {
            throw new NotFoundException($"Membership request of user {userId} does not exist.");
        }
        return request;
    }

    private void SetDetails(string name, string description, List<string>? images)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A club requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A club requires a description.");
        }
        if (images is null || images.Count == 0)
        {
            throw new DomainException("A club requires at least one image.");
        }

        Name = name.Trim();
        Description = description;
        Images = images;
    }
}
