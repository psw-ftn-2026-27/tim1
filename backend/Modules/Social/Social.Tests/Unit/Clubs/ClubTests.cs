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

    public static TheoryData<Guid> UsersWhoCannotResolveRequests => [OrdinaryMember, Outsider];

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
}
