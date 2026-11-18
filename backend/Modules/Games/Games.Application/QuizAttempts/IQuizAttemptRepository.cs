using Games.Domain.QuizAttempts;

namespace Games.Application.QuizAttempts;

public interface IQuizAttemptRepository
{
    Task<QuizAttempt?> GetByPlayerAndQuizAsync(Guid playerId, Guid quizId);
    void Add(QuizAttempt attempt);
}
