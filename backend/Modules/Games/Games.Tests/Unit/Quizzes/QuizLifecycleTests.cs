using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public sealed class QuizLifecycleTests
{
    [Theory]
    [InlineData(QuizStatus.Draft, QuizStatus.Published)]
    [InlineData(QuizStatus.Published, QuizStatus.Retired)]
    [InlineData(QuizStatus.Retired, QuizStatus.Archived)]
    public void A_quiz_advances_to_the_next_state_and_keeps_its_content(QuizStatus startingStatus, QuizStatus nextStatus)
    {
        var quiz = CreateQuizInState(startingStatus);
        var expected = CaptureContent(quiz);
        var transition = GetTransition(quiz, nextStatus);

        transition();

        quiz.Status.Should().Be(nextStatus);
        CaptureContent(quiz).Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(QuizStatus.Published, QuizStatus.Published)]
    [InlineData(QuizStatus.Retired, QuizStatus.Published)]
    [InlineData(QuizStatus.Archived, QuizStatus.Published)]
    [InlineData(QuizStatus.Draft, QuizStatus.Retired)]
    [InlineData(QuizStatus.Retired, QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived, QuizStatus.Retired)]
    [InlineData(QuizStatus.Draft, QuizStatus.Archived)]
    [InlineData(QuizStatus.Published, QuizStatus.Archived)]
    [InlineData(QuizStatus.Archived, QuizStatus.Archived)]
    public void An_invalid_state_change_preserves_the_quiz(QuizStatus startingStatus, QuizStatus requestedStatus)
    {
        var quiz = CreateQuizInState(startingStatus);
        var expected = CaptureContent(quiz);
        var transition = GetTransition(quiz, requestedStatus);

        transition.Should().Throw<DomainException>();

        quiz.Status.Should().Be(startingStatus);
        CaptureContent(quiz).Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(1, 1, true)]
    [InlineData(1, 2, false)]
    public void An_incomplete_quiz_remains_a_draft(int questionCount, int answerCount, bool hasCorrectAnswer)
    {
        var answers = Enumerable.Range(0, answerCount)
            .Select(index => new QuizAnswer($"Answer {index + 1}", index == 0 && hasCorrectAnswer))
            .ToArray();
        var questions = Enumerable.Range(0, questionCount)
            .Select(index => new QuizQuestion($"Question {index + 1}", answers))
            .ToArray();
        var quiz = new Quiz("City walk", "Read the signs.", 10, questions);
        var expected = CaptureContent(quiz);
        Action publish = quiz.Publish;

        publish.Should().Throw<DomainException>();

        quiz.Status.Should().Be(QuizStatus.Draft);
        CaptureContent(quiz).Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Archived)]
    public void A_draft_or_archived_quiz_can_be_deleted(QuizStatus status)
    {
        var quiz = CreateQuizInState(status);
        Action delete = quiz.Delete;

        delete.Should().NotThrow();
    }

    [Theory]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Retired)]
    public void A_published_or_retired_quiz_cannot_be_deleted(QuizStatus status)
    {
        var quiz = CreateQuizInState(status);
        var expected = CaptureContent(quiz);
        Action delete = quiz.Delete;

        delete.Should().Throw<DomainException>();

        quiz.Status.Should().Be(status);
        CaptureContent(quiz).Should().BeEquivalentTo(expected);
    }

    private static Quiz CreateQuizInState(QuizStatus status)
    {
        var quiz = new Quiz("City walk", "Read the signs.", 10,
            [new QuizQuestion("Which sign?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)])]);
        if (status is QuizStatus.Published or QuizStatus.Retired or QuizStatus.Archived)
        {
            quiz.Publish();
        }
        if (status is QuizStatus.Retired or QuizStatus.Archived)
        {
            quiz.Retire();
        }
        if (status == QuizStatus.Archived)
        {
            quiz.Archive();
        }

        return quiz;
    }

    private static Action GetTransition(Quiz quiz, QuizStatus requestedStatus)
    {
        return requestedStatus switch
        {
            QuizStatus.Published => quiz.Publish,
            QuizStatus.Retired => quiz.Retire,
            QuizStatus.Archived => quiz.Archive,
            _ => throw new ArgumentOutOfRangeException(nameof(requestedStatus))
        };
    }

    private static object CaptureContent(Quiz quiz)
    {
        return new
        {
            quiz.Name,
            quiz.Instructions,
            quiz.ExperiencePoints,
            quiz.CreatedAt,
            Questions = quiz.Questions.Select(question => new { question.Id, question.Text, Answers = question.Answers.ToArray() }).ToArray()
        };
    }
}
