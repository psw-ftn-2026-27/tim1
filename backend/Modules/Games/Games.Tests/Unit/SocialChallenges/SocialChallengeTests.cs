using System.Globalization;
using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private static SocialChallenge CreateChallenge() =>
        new("Večernja šetnja kejom", new TimeOnly(19, 0), new TimeOnly(21, 0), 5, 150, 50, "Bravo, zajedno ste prošetali kej!", 45.2466, 19.8556);

    // Walks a new challenge forward through the lifecycle, which follows the order of the
    // enum, until it reaches the given status.
    private static SocialChallenge CreateChallengeIn(SocialChallengeStatus status)
    {
        var challenge = CreateChallenge();
        if (status >= SocialChallengeStatus.Published)
        {
            challenge.Publish();
        }
        if (status >= SocialChallengeStatus.Retired)
        {
            challenge.Retire();
        }
        if (status >= SocialChallengeStatus.Archived)
        {
            challenge.Archive();
        }
        return challenge;
    }

    [Fact]
    public void New_social_challenge_starts_as_a_draft()
    {
        var challenge = CreateChallenge();

        challenge.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Theory]
    [InlineData("", "Bravo, zajedno ste prošetali kej!")]
    [InlineData("   ", "Bravo, zajedno ste prošetali kej!")]
    [InlineData("Večernja šetnja kejom", "")]
    [InlineData("Večernja šetnja kejom", "   ")]
    public void New_social_challenge_requires_a_name_and_a_completion_message(string name, string completionMessage)
    {
        var creation = () => new SocialChallenge(name, new TimeOnly(19, 0), new TimeOnly(21, 0), 5, 150, 50, completionMessage, 45.2466, 19.8556);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("19:00", "19:00")]
    [InlineData("19:30", "19:15")]
    [InlineData("22:00", "02:00")]
    public void New_social_challenge_must_start_before_it_ends_within_the_same_day(string activeFrom, string activeUntil)
    {
        var creation = () => new SocialChallenge(
            "Večernja šetnja kejom",
            TimeOnly.Parse(activeFrom, CultureInfo.InvariantCulture),
            TimeOnly.Parse(activeUntil, CultureInfo.InvariantCulture),
            5,
            150,
            50,
            "Bravo, zajedno ste prošetali kej!",
            45.2466,
            19.8556);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-3)]
    public void New_social_challenge_requires_at_least_two_participants(int requiredParticipants)
    {
        var creation = () => new SocialChallenge("Večernja šetnja kejom", new TimeOnly(19, 0), new TimeOnly(21, 0), requiredParticipants, 150, 50, "Bravo, zajedno ste prošetali kej!", 45.2466, 19.8556);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_social_challenge_can_require_exactly_two_participants()
    {
        var challenge = new SocialChallenge("Večernja šetnja kejom", new TimeOnly(19, 0), new TimeOnly(21, 0), 2, 150, 50, "Bravo, zajedno ste prošetali kej!", 45.2466, 19.8556);

        challenge.RequiredParticipants.Should().Be(2);
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(-10, 50)]
    [InlineData(150, 0)]
    [InlineData(150, -5)]
    public void New_social_challenge_requires_a_radius_and_experience_points_greater_than_zero(int radiusMeters, int experiencePoints)
    {
        var creation = () => new SocialChallenge("Večernja šetnja kejom", new TimeOnly(19, 0), new TimeOnly(21, 0), 5, radiusMeters, experiencePoints, "Bravo, zajedno ste prošetali kej!", 45.2466, 19.8556);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null, 19.8556)]
    [InlineData(45.2466, null)]
    [InlineData(null, null)]
    public void New_social_challenge_requires_a_location(double? latitude, double? longitude)
    {
        var creation = () => new SocialChallenge("Večernja šetnja kejom", new TimeOnly(19, 0), new TimeOnly(21, 0), 5, 150, 50, "Bravo, zajedno ste prošetali kej!", latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90.0001, 19.8556)]
    [InlineData(90.0001, 19.8556)]
    [InlineData(45.2466, -180.0001)]
    [InlineData(45.2466, 180.0001)]
    public void New_social_challenge_requires_a_latitude_and_longitude_within_their_ranges(double latitude, double longitude)
    {
        var creation = () => new SocialChallenge("Večernja šetnja kejom", new TimeOnly(19, 0), new TimeOnly(21, 0), 5, 150, 50, "Bravo, zajedno ste prošetali kej!", latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90.0, -180.0)]
    [InlineData(90.0, 180.0)]
    [InlineData(0.0, 0.0)]
    public void New_social_challenge_can_be_located_on_the_edges_of_the_ranges(double latitude, double longitude)
    {
        var challenge = new SocialChallenge("Večernja šetnja kejom", new TimeOnly(19, 0), new TimeOnly(21, 0), 5, 150, 50, "Bravo, zajedno ste prošetali kej!", latitude, longitude);

        challenge.Latitude.Should().Be(latitude);
        challenge.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Draft_social_challenge_is_published()
    {
        var challenge = CreateChallenge();

        challenge.Publish();

        challenge.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Retired)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Only_a_draft_social_challenge_is_published(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var publishing = () => challenge.Publish();

        publishing.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Fact]
    public void Published_social_challenge_is_retired()
    {
        var challenge = CreateChallengeIn(SocialChallengeStatus.Published);

        challenge.Retire();

        challenge.Status.Should().Be(SocialChallengeStatus.Retired);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Draft)]
    [InlineData(SocialChallengeStatus.Retired)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Only_a_published_social_challenge_is_retired(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var retiring = () => challenge.Retire();

        retiring.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Fact]
    public void Retired_social_challenge_is_archived()
    {
        var challenge = CreateChallengeIn(SocialChallengeStatus.Retired);

        challenge.Archive();

        challenge.Status.Should().Be(SocialChallengeStatus.Archived);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Draft)]
    [InlineData(SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Only_a_retired_social_challenge_is_archived(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var archiving = () => challenge.Archive();

        archiving.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Fact]
    public void Draft_social_challenge_is_changed()
    {
        var challenge = CreateChallenge();

        challenge.Update("Jutarnja šetnja kejom", new TimeOnly(7, 0), new TimeOnly(8, 30), 3, 200, 60, "Dobro jutro, izazov je završen!", 45.2551, 19.8451);

        challenge.Name.Should().Be("Jutarnja šetnja kejom");
        challenge.ActiveFrom.Should().Be(new TimeOnly(7, 0));
        challenge.ActiveUntil.Should().Be(new TimeOnly(8, 30));
        challenge.RequiredParticipants.Should().Be(3);
        challenge.RadiusMeters.Should().Be(200);
        challenge.ExperiencePoints.Should().Be(60);
        challenge.CompletionMessage.Should().Be("Dobro jutro, izazov je završen!");
        challenge.Latitude.Should().Be(45.2551);
        challenge.Longitude.Should().Be(19.8451);
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Retired)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Only_a_draft_social_challenge_is_changed(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var update = () => challenge.Update("Jutarnja šetnja kejom", new TimeOnly(7, 0), new TimeOnly(8, 30), 3, 200, 60, "Dobro jutro, izazov je završen!", 45.2551, 19.8451);

        update.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Večernja šetnja kejom");
        challenge.ActiveFrom.Should().Be(new TimeOnly(19, 0));
    }

    [Fact]
    public void Draft_social_challenge_is_not_changed_to_end_before_it_starts()
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update("Večernja šetnja kejom", new TimeOnly(21, 0), new TimeOnly(19, 0), 5, 150, 50, "Bravo, zajedno ste prošetali kej!", 45.2466, 19.8556);

        update.Should().Throw<DomainException>();
        challenge.ActiveFrom.Should().Be(new TimeOnly(19, 0));
        challenge.ActiveUntil.Should().Be(new TimeOnly(21, 0));
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Draft)]
    [InlineData(SocialChallengeStatus.Archived)]
    public void Draft_or_archived_social_challenge_can_be_deleted(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletion = () => challenge.Delete();

        deletion.Should().NotThrow();
    }

    [Theory]
    [InlineData(SocialChallengeStatus.Published)]
    [InlineData(SocialChallengeStatus.Retired)]
    public void Published_or_retired_social_challenge_cannot_be_deleted(SocialChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletion = () => challenge.Delete();

        deletion.Should().Throw<DomainException>();
    }
}
