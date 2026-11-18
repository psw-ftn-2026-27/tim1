using Games.Application.QuizManagement;
using Games.Application.Quizzes;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizReadRepository : IQuizReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<QuizSummaryDto>> GetAllAsync(int page, int pageSize)
    {
        var quizzes = _dbContext.Quizzes.OrderBy(quiz => quiz.Name).ThenBy(quiz => quiz.Id);
        var totalCount = await quizzes.CountAsync();
        var items = await quizzes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(quiz => new QuizSummaryDto(quiz.Id, quiz.Name, quiz.Status))
            .ToListAsync();

        return new PageResult<QuizSummaryDto>(items, totalCount);
    }

    public Task<QuizDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.Quizzes
            .AsNoTracking()
            .Where(quiz => quiz.Id == id)
            .Select(quiz => new QuizDto(
                quiz.Id,
                quiz.Name,
                quiz.Instructions,
                quiz.ExperiencePoints,
                quiz.Status,
                quiz.CreatedAt,
                quiz.Questions.OrderBy(question => question.Position)
                    .Select(question => new QuizQuestionDto(
                        question.Id,
                        question.Text,
                        question.Answers.Select(answer => new QuizAnswerDto(answer.Text, answer.IsCorrect)).ToList()))
                    .ToList(),
                quiz.Latitude,
                quiz.Longitude))
            .SingleOrDefaultAsync();
    }
}
