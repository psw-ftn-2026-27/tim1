using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static Club CreateClub() =>
        new(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarenja.", ["https://example.com/planinari.jpg"]);

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
}
