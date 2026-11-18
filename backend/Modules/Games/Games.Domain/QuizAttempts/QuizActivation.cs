using Games.Domain.ChallengeCompletions;
using Games.Domain.GeoPositions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Domain.QuizAttempts;

/// <summary>
/// Starts a player's attempt at a quiz: the quiz must be published and not solved by the player yet.
/// An attempt already in progress is continued, so nothing new is returned and the distance is not
/// checked again. Otherwise the player must be within reach of the quiz and gets a new attempt.
/// </summary>
public sealed class QuizActivation
{
    public QuizAttempt? Activate(
        Guid playerId,
        Quiz quiz,
        GeoPosition position,
        QuizAttempt? existingAttempt,
        ChallengeCompletion? existingCompletion,
        DateTime now)
    {
        if (quiz.Status != QuizStatus.Published)
        {
            throw new DomainException("The quiz is not available.");
        }

        if (existingCompletion is not null)
        {
            throw new DomainException("You have already solved this quiz.");
        }

        if (existingAttempt is not null)
        {
            return null;
        }

        if (!quiz.IsWithinReach(position))
        {
            throw new DomainException("You are too far from the quiz. Get closer to activate it.");
        }

        return new QuizAttempt(playerId, quiz.Id, now);
    }
}
