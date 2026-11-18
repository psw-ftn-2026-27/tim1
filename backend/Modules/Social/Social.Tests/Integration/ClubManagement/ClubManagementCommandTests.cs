using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubManagement;
using Social.Application.Clubs;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubManagement;

public class ClubManagementCommandTests : BaseIntegrationTest
{
    public ClubManagementCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Founder_creates_a_club_they_own()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateClubDto("Šetači", "Klub ljubitelja šetnje kejom.", ["https://example.com/setaci.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<ClubDto>(JsonOptions);
        created!.OwnerId.Should().Be(WellKnownUsers.Explorer);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore + 1);
        var stored = assertContext.Clubs.Single(club => club.Id == created.Id);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.Name.Should().Be("Šetači");
        stored.Images.Should().ContainSingle().Which.Should().Be("https://example.com/setaci.jpg");
    }

    [Fact]
    public async Task Founder_cannot_create_a_club_with_a_taken_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new CreateClubDto("  PLANINARI ", "Još jedan klub planinara.", ["https://example.com/planinari.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_club()
    {
        var client = Factory.CreateClient();
        var request = new CreateClubDto("Šetači", "Klub ljubitelja šetnje kejom.", ["https://example.com/setaci.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateClubDto("Šetači", "Klub ljubitelja šetnje kejom.", ["https://example.com/setaci.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Founder_updates_their_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Planinari Fruške gore", "Novi opis kluba.", ["https://example.com/fruska-gora.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.HikersClub.Id);
        stored.Name.Should().Be("Planinari Fruške gore");
        stored.Description.Should().Be("Novi opis kluba.");
        stored.Images.Should().ContainSingle().Which.Should().Be("https://example.com/fruska-gora.jpg");
    }

    [Fact]
    public async Task Founder_cannot_update_another_founders_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new UpdateClubDto("Preuzeti planinari", "Izmena tuđeg kluba.", ["https://example.com/tudji.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.HikersClub.Id);
        stored.Name.Should().Be(ClubSeed.HikersClub.Name);
        stored.Description.Should().Be(ClubSeed.HikersClub.Description);
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_club()
    {
        var client = Factory.CreateClient();
        var request = new UpdateClubDto("Planinari Fruške gore", "Novi opis kluba.", ["https://example.com/fruska-gora.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_update_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateClubDto("Planinari Fruške gore", "Novi opis kluba.", ["https://example.com/fruska-gora.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Founder_deletes_their_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Id == ClubSeed.HikersClub.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Founder_cannot_delete_another_founders_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Id == ClubSeed.HikersClub.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_delete_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_closes_their_club_and_its_pending_requests_and_invitations_are_gone()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.SkatersClubWithSecondExplorersPendingRequestAndThirdExplorersPendingInvitation.Id;

        var response = await client.PutAsync($"/api/social/clubs/{clubId}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Status.Should().Be(ClubStatus.Closed);
        stored.MembershipRequests.Should().BeEmpty();
        stored.Invitations.Should().BeEmpty();
    }

    [Fact]
    public async Task Club_that_does_not_exist_cannot_be_closed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsync($"/api/social/clubs/{Guid.NewGuid()}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task User_who_is_not_the_owner_cannot_close_the_club_and_it_stays_active()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var clubId = ClubSeed.HikersClub.Id;

        var response = await client.PutAsync($"/api/social/clubs/{clubId}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Single(club => club.Id == clubId).Status.Should().Be(ClubStatus.Active);
    }

    [Fact]
    public async Task Anonymous_user_cannot_close_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_close_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reopen_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}/reopen", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reopen_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}/reopen", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
