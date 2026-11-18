using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public sealed class QuizTests
{
    [Fact]
    public void New_quiz_can_be_saved_as_a_draft_with_incomplete_content()
    {
        var quiz = new Quiz("City walk", "Read the signs.", 0, []);

        quiz.Status.Should().Be(QuizStatus.Draft);
        quiz.Questions.Should().BeEmpty();
        quiz.ExperiencePoints.Should().Be(0);

        quiz.Update(quiz.Name, quiz.Instructions, 0, [new QuizQuestion("", [new QuizAnswer("", false)])]);

        quiz.Questions.Single().Answers.Should().ContainSingle(answer => !answer.IsCorrect);
    }

    [Fact]
    public void A_question_cannot_have_two_correct_answers()
    {
        var create = () => new QuizQuestion("Which sign?", [new QuizAnswer("First", true), new QuizAnswer("Second", true)]);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null, "Read the signs.", 1)]
    [InlineData(" ", "Read the signs.", 1)]
    [InlineData("City walk", null, 1)]
    [InlineData("City walk", " ", 1)]
    [InlineData("City walk", "Read the signs.", null)]
    [InlineData("City walk", "Read the signs.", -1)]
    public void A_draft_requires_a_name_instructions_and_non_negative_experience_points(
        string? name, string? instructions, int? experiencePoints)
    {
        var quiz = new Quiz("Original", "Follow the trail.", 1, []);
        var create = () => new Quiz(name, instructions, experiencePoints, []);
        var change = () => quiz.Update(name, instructions, experiencePoints, []);

        create.Should().Throw<DomainException>();
        change.Should().Throw<DomainException>();
        quiz.Name.Should().Be("Original");
        quiz.Instructions.Should().Be("Follow the trail.");
        quiz.ExperiencePoints.Should().Be(1);
    }

    [Fact]
    public void A_published_quiz_cannot_be_changed()
    {
        var question = new QuizQuestion("Which sign?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)]);
        var quiz = new Quiz("City walk", "Read the signs.", 10, [question], 45, 19);
        quiz.Publish();

        var change = () => quiz.Update("New name", "New instructions.", 0, [], 46, 20);

        change.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Published);
        quiz.Name.Should().Be("City walk");
        quiz.Instructions.Should().Be("Read the signs.");
        quiz.ExperiencePoints.Should().Be(10);
        quiz.Latitude.Should().Be(45);
        quiz.Longitude.Should().Be(19);
        quiz.Questions.Should().ContainSingle().Which.Id.Should().Be(question.Id);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(45.25, 19.85)]
    [InlineData(-90d, -180d)]
    [InlineData(90d, 180d)]
    public void A_draft_can_have_a_valid_location_or_no_location(double? latitude, double? longitude)
    {
        var quiz = new Quiz("City walk", "Read the signs.", 0, [], latitude, longitude);

        quiz.Latitude.Should().Be(latitude);
        quiz.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(45d, null)]
    [InlineData(null, 19d)]
    [InlineData(-91d, 19d)]
    [InlineData(91d, 19d)]
    [InlineData(45d, -181d)]
    [InlineData(45d, 181d)]
    [InlineData(double.NaN, 19d)]
    [InlineData(45d, double.NaN)]
    [InlineData(double.PositiveInfinity, 19d)]
    [InlineData(45d, double.NegativeInfinity)]
    public void A_quiz_rejects_an_invalid_location(double? latitude, double? longitude)
    {
        var create = () => new Quiz("City walk", "Read the signs.", 0, [], latitude, longitude);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_invalid_location_leaves_the_entire_quiz_unchanged()
    {
        var question = new QuizQuestion("Which sign?", [new QuizAnswer("First", true)]);
        var quiz = new Quiz("City walk", "Read the signs.", 10, [question], 45, 19);
        var createdAt = quiz.CreatedAt;
        var editedQuestion = new QuizQuestion("Changed", [new QuizAnswer("Other", false)], question.Id);

        var change = () => quiz.Update("New name", "New instructions.", 0, [editedQuestion], 91, 19);

        change.Should().Throw<DomainException>();
        quiz.Name.Should().Be("City walk");
        quiz.Instructions.Should().Be("Read the signs.");
        quiz.ExperiencePoints.Should().Be(10);
        quiz.Status.Should().Be(QuizStatus.Draft);
        quiz.CreatedAt.Should().Be(createdAt);
        quiz.Latitude.Should().Be(45);
        quiz.Longitude.Should().Be(19);
        var savedQuestion = quiz.Questions.Should().ContainSingle().Which;
        savedQuestion.Id.Should().Be(question.Id);
        savedQuestion.Text.Should().Be("Which sign?");
        savedQuestion.Answers.Should().Equal(new QuizAnswer("First", true));
    }

    [Fact]
    public void Editing_a_quiz_preserves_its_utc_creation_time()
    {
        var before = DateTime.UtcNow;
        var quiz = new Quiz("City walk", "Read the signs.", 0, []);
        var createdAt = quiz.CreatedAt;

        quiz.Update("New city walk", quiz.Instructions, 0, []);

        createdAt.Kind.Should().Be(DateTimeKind.Utc);
        createdAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
        quiz.CreatedAt.Should().Be(createdAt);
    }
}
