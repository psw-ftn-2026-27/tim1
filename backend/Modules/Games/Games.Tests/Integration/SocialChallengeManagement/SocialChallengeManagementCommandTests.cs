using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeManagement;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeManagement;

public class SocialChallengeManagementCommandTests : BaseIntegrationTest
{
    public SocialChallengeManagementCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_social_challenge_that_starts_as_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateSocialChallengeDto("Kafa na Trgu slobode", new TimeOnly(8, 0), new TimeOnly(9, 30), 3, 120, 40, "Uživajte u kafi, izazov je završen!", 45.2551, 19.8451);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == createdId);
        stored.Name.Should().Be(request.Name);
        stored.ActiveFrom.Should().Be(request.ActiveFrom);
        stored.ActiveUntil.Should().Be(request.ActiveUntil);
        stored.RequiredParticipants.Should().Be(request.RequiredParticipants);
        stored.RadiusMeters.Should().Be(request.RadiusMeters);
        stored.ExperiencePoints.Should().Be(request.ExperiencePoints);
        stored.CompletionMessage.Should().Be(request.CompletionMessage);
        stored.Latitude.Should().Be(request.Latitude);
        stored.Longitude.Should().Be(request.Longitude);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_social_challenge()
    {
        var client = Factory.CreateClient();
        var request = new CreateSocialChallengeDto("Kafa na Trgu slobode", new TimeOnly(8, 0), new TimeOnly(9, 30), 3, 120, 40, "Uživajte u kafi, izazov je završen!", 45.2551, 19.8451);

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateSocialChallengeDto("Kafa na Trgu slobode", new TimeOnly(8, 0), new TimeOnly(9, 30), 3, 120, 40, "Uživajte u kafi, izazov je završen!", 45.2551, 19.8451);

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_changes_a_draft_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateSocialChallengeDto("Večernja šetnja Limanskim parkom", new TimeOnly(18, 30), new TimeOnly(20, 0), 4, 300, 70, "Lepa šetnja, izazov je završen!", 45.2433, 19.8385);

        var response = await client.PutAsJsonAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.DraftQuayWalk.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftQuayWalk.Id);
        stored.Name.Should().Be(request.Name);
        stored.ActiveFrom.Should().Be(request.ActiveFrom);
        stored.ActiveUntil.Should().Be(request.ActiveUntil);
        stored.RequiredParticipants.Should().Be(request.RequiredParticipants);
        stored.RadiusMeters.Should().Be(request.RadiusMeters);
        stored.ExperiencePoints.Should().Be(request.ExperiencePoints);
        stored.CompletionMessage.Should().Be(request.CompletionMessage);
        stored.Latitude.Should().Be(request.Latitude);
        stored.Longitude.Should().Be(request.Longitude);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public async Task Social_challenge_that_does_not_exist_cannot_be_changed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateSocialChallengeDto("Večernja šetnja Limanskim parkom", new TimeOnly(18, 30), new TimeOnly(20, 0), 4, 300, 70, "Lepa šetnja, izazov je završen!", 45.2433, 19.8385);

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Any(challenge => challenge.Name == request.Name).Should().BeFalse();
    }

    [Fact]
    public async Task Published_social_challenge_cannot_be_changed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateSocialChallengeDto("Joga na keju", new TimeOnly(7, 0), new TimeOnly(8, 30), 6, 80, 90, "Izazov je završen!", 45.2466, 19.8556);
        var seeded = SocialChallengeSeed.PublishedFortressYoga;

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{seeded.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == seeded.Id);
        stored.Name.Should().Be(seeded.Name);
        stored.ActiveFrom.Should().Be(seeded.ActiveFrom);
        stored.ActiveUntil.Should().Be(seeded.ActiveUntil);
        stored.RequiredParticipants.Should().Be(seeded.RequiredParticipants);
        stored.RadiusMeters.Should().Be(seeded.RadiusMeters);
        stored.ExperiencePoints.Should().Be(seeded.ExperiencePoints);
        stored.CompletionMessage.Should().Be(seeded.CompletionMessage);
        stored.Latitude.Should().Be(seeded.Latitude);
        stored.Longitude.Should().Be(seeded.Longitude);
        stored.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Administrator_publishes_a_draft_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.DraftQuayWalk.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftQuayWalk.Id)
            .Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Administrator_retires_a_published_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.PublishedFortressYoga.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedFortressYoga.Id)
            .Status.Should().Be(SocialChallengeStatus.Retired);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.RetiredParkRun.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.RetiredParkRun.Id)
            .Status.Should().Be(SocialChallengeStatus.Archived);
    }

    [Fact]
    public async Task Administrator_deletes_an_archived_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.ArchivedBeachPicnic.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Any(challenge => challenge.Id == SocialChallengeSeed.ArchivedBeachPicnic.Id)
            .Should().BeFalse();
    }
}
