using Games.Application.QuizAttempts;
using Games.Domain.QuizAttempts;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.QuizAttempts;

internal sealed class QuizAttemptRepository : IQuizAttemptRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizAttemptRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<QuizAttempt?> GetByPlayerAndQuizAsync(Guid playerId, Guid quizId)
    {
        return _dbContext.QuizAttempts
            .SingleOrDefaultAsync(attempt => attempt.PlayerId == playerId && attempt.QuizId == quizId);
    }

    public void Add(QuizAttempt attempt)
    {
        _dbContext.QuizAttempts.Add(attempt);
    }
}
