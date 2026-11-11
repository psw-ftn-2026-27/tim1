using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.Clubs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubBrowsing;

public class ClubBrowsingQueryTests : BaseIntegrationTest
{
    public ClubBrowsingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_sees_the_clubs_of_all_founders()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var clubs = await client.GetFromJsonAsync<PageResult<ClubDto>>("/api/social/clubs", JsonOptions);

        clubs!.Items.Select(club => club.Id).Should().BeEquivalentTo(ClubSeed.All.Select(club => club.Id));
        clubs.TotalCount.Should().Be(ClubSeed.All.Length);
    }

    [Fact]
    public async Task User_sees_the_name_description_and_all_images_of_the_chosen_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var club = await client.GetFromJsonAsync<ClubDto>(
            $"/api/social/clubs/{ClubSeed.PhotographersClub.Id}", JsonOptions);

        club!.Id.Should().Be(ClubSeed.PhotographersClub.Id);
        club.Name.Should().Be(ClubSeed.PhotographersClub.Name);
        club.Description.Should().Be(ClubSeed.PhotographersClub.Description);
        club.Images.Should().Equal(ClubSeed.PhotographersClub.Images);
    }

    [Fact]
    public async Task Club_that_does_not_exist_is_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/clubs/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
