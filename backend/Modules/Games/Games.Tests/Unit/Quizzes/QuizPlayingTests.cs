using FluentAssertions;
using Games.Domain.GeoPositions;
using Games.Domain.Quizzes;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public sealed class QuizPlayingTests
{
    [Fact]
    public void A_quiz_without_a_location_is_within_reach_of_every_position()
    {
        var quiz = CreateQuiz(10, 1);

        var withinReach = quiz.IsWithinReach(new GeoPosition(-33, 151));

        withinReach.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(0.00044, true)]
    [InlineData(0.00046, false)]
    [InlineData(0.01, false)]
    public void A_quiz_with_a_location_is_within_reach_only_up_to_fifty_metres(double latitudeOffset, bool expected)
    {
        var quiz = CreateQuiz(10, 1, 45, 19);

        var withinReach = quiz.IsWithinReach(new GeoPosition(45 + latitudeOffset, 19));

        withinReach.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 4, 0, 0)]
    [InlineData(10, 4, 1, 3)]
    [InlineData(10, 4, 2, 5)]
    [InlineData(10, 4, 3, 8)]
    [InlineData(10, 4, 4, 10)]
    [InlineData(5, 3, 1, 2)]
    [InlineData(5, 3, 2, 3)]
    public void Experience_points_are_proportional_to_correct_answers_with_a_half_rounded_up(
        int experiencePoints, int questionCount, int correctAnswers, int expected)
    {
        var quiz = CreateQuiz(experiencePoints, questionCount);

        var points = quiz.ExperiencePointsFor(correctAnswers);

        points.Should().Be(expected);
    }

    private static Quiz CreateQuiz(int experiencePoints, int questionCount, double? latitude = null, double? longitude = null)
    {
        var questions = Enumerable.Range(0, questionCount)
            .Select(index => new QuizQuestion($"Question {index + 1}",
                [new QuizAnswer("First", true), new QuizAnswer("Second", false)]))
            .ToArray();
        var quiz = new Quiz("City walk", "Read the signs.", experiencePoints, questions, latitude, longitude);
        quiz.Publish();

        return quiz;
    }
}
