using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.FollowRequests;
using Xunit;

namespace Social.Tests.Unit.FollowRequests;

public class FollowRequestTests
{
    private static FollowRequest CreateRequest() =>
        new(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

    [Fact]
    public void User_cannot_send_a_follow_request_to_themselves()
    {
        var creation = () => new FollowRequest(WellKnownUsers.Explorer, WellKnownUsers.Explorer);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Follow_request_is_not_sent_while_the_first_one_is_pending()
    {
        var pending = CreateRequest();

        var sending = () => FollowRequestSending.Send(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, [pending]);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void Follow_request_is_not_sent_to_a_user_who_is_already_followed()
    {
        var accepted = CreateRequest();
        accepted.Accept();

        var sending = () => FollowRequestSending.Send(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, [accepted]);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void Follow_request_can_be_sent_again_after_a_rejection()
    {
        var rejected = CreateRequest();
        rejected.Reject();

        var request = FollowRequestSending.Send(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, [rejected]);

        request.Status.Should().Be(FollowRequestStatus.Pending);
    }

    [Fact]
    public void Follow_request_to_a_user_who_already_follows_the_sender_is_accepted_at_once()
    {
        var recipientFollowsSender = new FollowRequest(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);
        recipientFollowsSender.Accept();

        var request = FollowRequestSending.Send(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, [recipientFollowsSender]);

        request.Status.Should().Be(FollowRequestStatus.Accepted);
    }

    [Fact]
    public void Recipient_cannot_delete_a_follow_request()
    {
        var request = CreateRequest();

        var deletion = () => request.Delete(WellKnownUsers.SecondExplorer);

        deletion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Sender_cannot_delete_an_accepted_follow_request()
    {
        var request = CreateRequest();
        request.Accept();

        var deletion = () => request.Delete(WellKnownUsers.Explorer);

        deletion.Should().Throw<DomainException>();
    }
}
