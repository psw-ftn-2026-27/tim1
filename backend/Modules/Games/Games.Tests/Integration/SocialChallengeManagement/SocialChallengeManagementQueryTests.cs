using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeManagement;
using Games.Domain.SocialChallenges;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeManagement;

public class SocialChallengeManagementQueryTests : BaseIntegrationTest
{
    public SocialChallengeManagementQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_all_social_challenges_ordered_by_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var expectedOrder = SocialChallengeSeed.All
            .OrderBy(challenge => challenge.Name, StringComparer.Ordinal)
            .Select(challenge => challenge.Id);

        var challenges = await client.GetFromJsonAsync<PageResult<SocialChallengeDto>>(
            "/api/games/social-challenges", JsonOptions);

        challenges!.Items.Select(challenge => challenge.Id).Should().Equal(expectedOrder);
        challenges.TotalCount.Should().Be(SocialChallengeSeed.All.Length);
    }

    [Fact]
    public async Task Administrator_sees_every_detail_of_the_chosen_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = SocialChallengeSeed.PublishedFortressYoga;

        var challenge = await client.GetFromJsonAsync<SocialChallengeDto>(
            $"/api/games/social-challenges/{seeded.Id}", JsonOptions);

        challenge!.Id.Should().Be(seeded.Id);
        challenge.Name.Should().Be(seeded.Name);
        challenge.ActiveFrom.Should().Be(seeded.ActiveFrom);
        challenge.ActiveUntil.Should().Be(seeded.ActiveUntil);
        challenge.RequiredParticipants.Should().Be(seeded.RequiredParticipants);
        challenge.RadiusMeters.Should().Be(seeded.RadiusMeters);
        challenge.ExperiencePoints.Should().Be(seeded.ExperiencePoints);
        challenge.CompletionMessage.Should().Be(seeded.CompletionMessage);
        challenge.Latitude.Should().Be(seeded.Latitude);
        challenge.Longitude.Should().Be(seeded.Longitude);
        challenge.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Social_challenge_that_does_not_exist_is_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"/api/games/social-challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_social_challenges()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/games/social-challenges");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_see_social_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync("/api/games/social-challenges");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
