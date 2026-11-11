using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubNameUniquenessTests
{
    private static Club CreateClub(string name) =>
        new(WellKnownUsers.Explorer, name, "Klub ljubitelja planinarenja.", ["https://example.com/planinari.jpg"]);

    [Theory]
    [InlineData("Planinari")]
    [InlineData("PLANINARI")]
    [InlineData("  planinari  ")]
    public void Name_is_taken_when_it_matches_an_existing_club_regardless_of_case_and_surrounding_spaces(string name)
    {
        var existingClub = CreateClub("Planinari");
        var newClub = CreateClub(name);

        var check = () => ClubNameUniqueness.EnsureNameIsFree(newClub, existingClub);

        check.Should().Throw<DomainException>();
    }

    [Fact]
    public void Club_keeps_its_own_name_on_update()
    {
        var club = CreateClub("Planinari");
        club.Update("PLANINARI", "Novi opis kluba.", ["https://example.com/planinari.jpg"]);

        var check = () => ClubNameUniqueness.EnsureNameIsFree(club, club);

        check.Should().NotThrow();
    }
}
