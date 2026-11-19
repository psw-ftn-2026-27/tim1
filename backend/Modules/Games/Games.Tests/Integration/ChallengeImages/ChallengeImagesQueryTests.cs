using System.Net;
using FluentAssertions;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.ChallengeImages;

public sealed class ChallengeImagesQueryTests : BaseIntegrationTest
{
    public ChallengeImagesQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Uploaded_photo_is_returned_by_its_identifier()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"/api/games/images/{ImageSeed.FortressGatePhoto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/webp");
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().Equal(GamesApiFactory.SeededImageContent);
    }
}
