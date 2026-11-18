using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizRepository : IQuizRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Quiz?> GetByIdAsync(Guid id)
    {
        return _dbContext.Quizzes.SingleOrDefaultAsync(quiz => quiz.Id == id);
    }

    public void Add(Quiz quiz)
    {
        _dbContext.Quizzes.Add(quiz);
    }

    public void Remove(Quiz quiz)
    {
        _dbContext.Quizzes.Remove(quiz);
    }
}
