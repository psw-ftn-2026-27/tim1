using Games.Domain.ChallengeCompletions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Domain.QuizAttempts;

/// <summary>
/// Takes an answer to the current question of an attempt. The last answer completes the attempt
/// and hands back the completion that awards the experience points; any other answer returns nothing.
/// </summary>
public sealed class QuizAnswering
{
    /// <summary>The first question by position without an answer in the attempt, or null when all are answered.</summary>
    public QuizQuestion? CurrentQuestion(QuizAttempt attempt, Quiz quiz)
    {
        return quiz.Questions
            .OrderBy(question => question.Position)
            .FirstOrDefault(question => attempt.Answers.All(answer => answer.QuestionId != question.Id));
    }

    public ChallengeCompletion? Answer(QuizAttempt attempt, Quiz quiz, Guid questionId, int answerIndex, DateTime now)
    {
        if (quiz.Status != QuizStatus.Published)
        {
            throw new DomainException("The quiz is not available.");
        }

        var current = CurrentQuestion(attempt, quiz);
        if (current is null || current.Id != questionId)
        {
            throw new DomainException("This is not the current question.");
        }

        if (answerIndex < 0 || answerIndex >= current.Answers.Count)
        {
            throw new DomainException("The answer is not one of the offered answers.");
        }

        attempt.AddAnswer(new GivenAnswer(questionId, answerIndex, current.Answers[answerIndex].IsCorrect, now));

        if (CurrentQuestion(attempt, quiz) is not null)
        {
            return null;
        }

        attempt.Complete(now);
        return new ChallengeCompletion(
            attempt.PlayerId, quiz.Id, ChallengeType.Quiz, now, quiz.ExperiencePointsFor(attempt.Points));
    }
}
