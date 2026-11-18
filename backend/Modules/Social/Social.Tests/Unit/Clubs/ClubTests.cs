using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static readonly Guid Owner = WellKnownUsers.Explorer;
    private static readonly Guid Requester = WellKnownUsers.SecondExplorer;
    private static readonly Guid OrdinaryMember = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private static readonly Guid Outsider = Guid.Parse("00000000-0000-0000-0000-000000000011");
    private static readonly Guid Invitee = Guid.Parse("00000000-0000-0000-0000-000000000012");
    private static readonly Guid Moderator = Guid.Parse("00000000-0000-0000-0000-000000000013");
    private static readonly Guid SecondModerator = Guid.Parse("00000000-0000-0000-0000-000000000014");
    private static readonly Guid SecondOrdinaryMember = Guid.Parse("00000000-0000-0000-0000-000000000015");

    public static TheoryData<Guid> UsersWhoCannotResolveRequests => [OrdinaryMember, Outsider];
    public static TheoryData<Guid> UsersWhoCannotInvite => [OrdinaryMember, Outsider];
    public static TheoryData<Guid> UsersWhoCannotBeInvited => [OrdinaryMember, Invitee, Requester];
    public static TheoryData<Guid> UsersWhoAreNotTheOwner => [Moderator, OrdinaryMember, Outsider];
    public static TheoryData<Guid> UsersWhoAreNotOrdinaryMembers => [Owner, Moderator, Outsider];
    public static TheoryData<Guid> UsersWhoCannotReceiveOwnership => [Owner, Outsider];
    public static TheoryData<Guid> MembersTheOwnerCanRemove => [Moderator, OrdinaryMember];
    public static TheoryData<Guid> MembersAModeratorCannotRemove => [Owner, SecondModerator];
    public static TheoryData<Guid> MembersAnOrdinaryMemberCannotRemove => [Owner, Moderator, SecondOrdinaryMember];

    private static Club CreateClub() =>
        new(Owner, "Planinari", "Klub ljubitelja planinarenja.", ["https://example.com/planinari.jpg"]);

    private static Club CreateClubWithOrdinaryMemberAndPendingRequest()
    {
        var club = CreateClub();
        club.RequestMembership(OrdinaryMember);
        club.AcceptMembershipRequest(Owner, OrdinaryMember);
        club.RequestMembership(Requester);
        return club;
    }

    private static Club CreateClubWithOrdinaryMemberPendingRequestAndPendingInvitation()
    {
        var club = CreateClubWithOrdinaryMemberAndPendingRequest();
        club.InviteMember(Owner, Invitee);
        return club;
    }

    private static Club CreateClubWithModeratorOrdinaryMemberAndPendingRequest()
    {
        var club = CreateClubWithOrdinaryMemberAndPendingRequest();
        club.RequestMembership(Moderator);
        club.AcceptMembershipRequest(Owner, Moderator);
        club.PromoteToModerator(Owner, Moderator);
        return club;
    }

    private static Club CreateClubWithTwoModeratorsAndTwoOrdinaryMembers()
    {
        var club = CreateClubWithModeratorOrdinaryMemberAndPendingRequest();
        club.RequestMembership(SecondModerator);
        club.AcceptMembershipRequest(Owner, SecondModerator);
        club.PromoteToModerator(Owner, SecondModerator);
        club.RequestMembership(SecondOrdinaryMember);
        club.AcceptMembershipRequest(Owner, SecondOrdinaryMember);
        return club;
    }

    [Theory]
    [InlineData("", "Klub ljubitelja planinarenja.")]
    [InlineData("   ", "Klub ljubitelja planinarenja.")]
    [InlineData("Planinari", "")]
    [InlineData("Planinari", "   ")]
    public void New_club_requires_a_name_and_a_description(string name, string description)
    {
        var creation = () => new Club(WellKnownUsers.Explorer, name, description, ["https://example.com/planinari.jpg"]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Novi opis kluba.")]
    [InlineData("   ", "Novi opis kluba.")]
    [InlineData("Novi planinari", "")]
    [InlineData("Novi planinari", "   ")]
    public void Club_cannot_lose_its_name_or_description(string name, string description)
    {
        var club = CreateClub();

        var update = () => club.Update(name, description, ["https://example.com/planinari.jpg"]);

        update.Should().Throw<DomainException>();
        club.Name.Should().Be("Planinari");
        club.Description.Should().Be("Klub ljubitelja planinarenja.");
    }

    [Fact]
    public void New_club_requires_an_image()
    {
        var creation = () => new Club(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarenja.", []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Club_cannot_lose_all_its_images()
    {
        var club = CreateClub();

        var update = () => club.Update("Planinari", "Klub ljubitelja planinarenja.", []);

        update.Should().Throw<DomainException>();
        club.Images.Should().ContainSingle().Which.Should().Be("https://example.com/planinari.jpg");
    }

    [Fact]
    public void Founder_is_a_member_with_the_owner_role_after_creating_the_club()
    {
        var club = new Club(Owner, "Planinari", "Klub ljubitelja planinarenja.", ["https://example.com/planinari.jpg"]);

        club.Members.Should().ContainSingle(member => member.UserId == Owner)
            .Which.Role.Should().Be(ClubRole.Owner);
    }

    [Fact]
    public void Club_member_cannot_request_membership_in_the_same_club()
    {
        var club = CreateClub();

        var request = () => club.RequestMembership(Owner);

        request.Should().Throw<DomainException>();
        club.MembershipRequests.Should().BeEmpty();
    }

    [Fact]
    public void User_with_a_pending_request_cannot_send_another_request_for_the_same_club()
    {
        var club = CreateClub();
        club.RequestMembership(Requester);

        var request = () => club.RequestMembership(Requester);

        request.Should().Throw<DomainException>();
        club.MembershipRequests.Should().ContainSingle(request => request.UserId == Requester);
    }

    [Theory]
    [MemberData(nameof(UsersWhoCannotResolveRequests))]
    public void Ordinary_member_or_outsider_cannot_accept_a_request(Guid resolverId)
    {
        var club = CreateClubWithOrdinaryMemberAndPendingRequest();

        var acceptance = () => club.AcceptMembershipRequest(resolverId, Requester);

        acceptance.Should().Throw<DomainException>();
        club.MembershipRequests.Should().Contain(request => request.UserId == Requester);
        club.Members.Should().NotContain(member => member.UserId == Requester);
    }

    [Theory]
    [MemberData(nameof(UsersWhoCannotResolveRequests))]
    public void Ordinary_member_or_outsider_cannot_reject_a_request(Guid resolverId)
    {
        var club = CreateClubWithOrdinaryMemberAndPendingRequest();

        var rejection = () => club.RejectMembershipRequest(resolverId, Requester);

        rejection.Should().Throw<DomainException>();
        club.MembershipRequests.Should().Contain(request => request.UserId == Requester);
    }

    [Fact]
    public void Accepted_requester_becomes_an_ordinary_member_and_their_request_is_gone()
    {
        var club = CreateClub();
        club.RequestMembership(Requester);

        club.AcceptMembershipRequest(Owner, Requester);

        club.Members.Should().ContainSingle(member => member.UserId == Requester)
            .Which.Role.Should().Be(ClubRole.Member);
        club.MembershipRequests.Should().NotContain(request => request.UserId == Requester);
    }

    [Fact]
    public void Request_that_no_longer_exists_cannot_be_accepted()
    {
        var club = CreateClub();
        club.RequestMembership(Requester);
        club.WithdrawMembershipRequest(Requester);

        var acceptance = () => club.AcceptMembershipRequest(Owner, Requester);

        acceptance.Should().Throw<NotFoundException>();
        club.Members.Should().NotContain(member => member.UserId == Requester);
    }

    [Theory]
    [MemberData(nameof(UsersWhoCannotInvite))]
    public void Only_the_club_owner_can_invite_a_user(Guid inviterId)
    {
        var club = CreateClubWithOrdinaryMemberAndPendingRequest();

        var invitation = () => club.InviteMember(inviterId, Invitee);

        invitation.Should().Throw<DomainException>();
        club.Invitations.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(UsersWhoCannotBeInvited))]
    public void Club_member_already_invited_user_or_user_with_a_pending_request_cannot_be_invited(Guid userId)
    {
        var club = CreateClubWithOrdinaryMemberPendingRequestAndPendingInvitation();

        var invitation = () => club.InviteMember(Owner, userId);

        invitation.Should().Throw<DomainException>();
        club.Invitations.Should().ContainSingle().Which.UserId.Should().Be(Invitee);
    }

    [Fact]
    public void User_with_a_pending_invitation_cannot_request_membership_in_the_same_club()
    {
        var club = CreateClub();
        club.InviteMember(Owner, Invitee);

        var request = () => club.RequestMembership(Invitee);

        request.Should().Throw<DomainException>();
        club.MembershipRequests.Should().BeEmpty();
    }

    [Fact]
    public void Invited_user_who_accepts_becomes_an_ordinary_member_and_their_invitation_is_gone()
    {
        var club = CreateClub();
        club.InviteMember(Owner, Invitee);

        club.AcceptInvitation(Invitee);

        club.Members.Should().ContainSingle(member => member.UserId == Invitee)
            .Which.Role.Should().Be(ClubRole.Member);
        club.Invitations.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(UsersWhoAreNotTheOwner))]
    public void Only_the_owner_can_promote_a_member_to_moderator(Guid promoterId)
    {
        var club = CreateClubWithModeratorOrdinaryMemberAndPendingRequest();

        var promotion = () => club.PromoteToModerator(promoterId, OrdinaryMember);

        promotion.Should().Throw<DomainException>();
        club.Members.Should().ContainSingle(member => member.UserId == OrdinaryMember)
            .Which.Role.Should().Be(ClubRole.Member);
    }

    [Theory]
    [MemberData(nameof(UsersWhoAreNotTheOwner))]
    public void Only_the_owner_can_transfer_ownership(Guid transferrerId)
    {
        var club = CreateClubWithModeratorOrdinaryMemberAndPendingRequest();

        var transfer = () => club.TransferOwnership(transferrerId, OrdinaryMember);

        transfer.Should().Throw<DomainException>();
        club.OwnerId.Should().Be(Owner);
        club.Members.Should().ContainSingle(member => member.Role == ClubRole.Owner)
            .Which.UserId.Should().Be(Owner);
    }

    [Theory]
    [MemberData(nameof(UsersWhoAreNotOrdinaryMembers))]
    public void Only_an_ordinary_member_can_be_promoted_to_moderator(Guid userId)
    {
        var club = CreateClubWithModeratorOrdinaryMemberAndPendingRequest();
        var rolesBefore = club.Members.Select(member => (member.UserId, member.Role)).ToList();

        var promotion = () => club.PromoteToModerator(Owner, userId);

        promotion.Should().Throw<DomainException>();
        club.Members.Select(member => (member.UserId, member.Role)).Should().BeEquivalentTo(rolesBefore);
    }

    [Theory]
    [MemberData(nameof(UsersWhoCannotReceiveOwnership))]
    public void Ownership_cannot_be_transferred_to_a_non_member_or_to_the_owner(Guid userId)
    {
        var club = CreateClubWithModeratorOrdinaryMemberAndPendingRequest();

        var transfer = () => club.TransferOwnership(Owner, userId);

        transfer.Should().Throw<DomainException>();
        club.OwnerId.Should().Be(Owner);
        club.Members.Should().ContainSingle(member => member.Role == ClubRole.Owner)
            .Which.UserId.Should().Be(Owner);
    }

    [Fact]
    public void After_the_transfer_the_moderator_is_the_only_owner_and_the_old_owner_is_an_ordinary_member()
    {
        var club = CreateClubWithModeratorOrdinaryMemberAndPendingRequest();

        club.TransferOwnership(Owner, Moderator);

        club.Members.Should().ContainSingle(member => member.Role == ClubRole.Owner)
            .Which.UserId.Should().Be(Moderator);
        club.Members.Should().NotContain(member => member.Role == ClubRole.Moderator);
        club.Members.Should().ContainSingle(member => member.UserId == Owner)
            .Which.Role.Should().Be(ClubRole.Member);
        club.OwnerId.Should().Be(Moderator);
    }

    [Fact]
    public void Moderator_accepts_a_request_and_the_requester_becomes_an_ordinary_member()
    {
        var club = CreateClubWithModeratorOrdinaryMemberAndPendingRequest();

        club.AcceptMembershipRequest(Moderator, Requester);

        club.Members.Should().ContainSingle(member => member.UserId == Requester)
            .Which.Role.Should().Be(ClubRole.Member);
        club.MembershipRequests.Should().NotContain(request => request.UserId == Requester);
    }

    [Fact]
    public void Owner_cannot_remove_themselves_from_the_club()
    {
        var club = CreateClubWithTwoModeratorsAndTwoOrdinaryMembers();

        var removal = () => club.RemoveMember(Owner, Owner);

        removal.Should().Throw<DomainException>();
        club.Members.Should().Contain(member => member.UserId == Owner);
    }

    [Theory]
    [MemberData(nameof(MembersTheOwnerCanRemove))]
    public void Owner_removes_a_moderator_or_an_ordinary_member_and_they_are_no_longer_a_member(Guid memberId)
    {
        var club = CreateClubWithTwoModeratorsAndTwoOrdinaryMembers();

        club.RemoveMember(Owner, memberId);

        club.Members.Should().NotContain(member => member.UserId == memberId);
    }

    [Theory]
    [MemberData(nameof(MembersAModeratorCannotRemove))]
    public void Moderator_cannot_remove_the_owner_or_another_moderator(Guid memberId)
    {
        var club = CreateClubWithTwoModeratorsAndTwoOrdinaryMembers();

        var removal = () => club.RemoveMember(Moderator, memberId);

        removal.Should().Throw<DomainException>();
        club.Members.Should().Contain(member => member.UserId == memberId);
    }

    [Theory]
    [MemberData(nameof(MembersAnOrdinaryMemberCannotRemove))]
    public void Ordinary_member_cannot_remove_anyone(Guid memberId)
    {
        var club = CreateClubWithTwoModeratorsAndTwoOrdinaryMembers();

        var removal = () => club.RemoveMember(OrdinaryMember, memberId);

        removal.Should().Throw<DomainException>();
        club.Members.Should().Contain(member => member.UserId == memberId);
    }
}
