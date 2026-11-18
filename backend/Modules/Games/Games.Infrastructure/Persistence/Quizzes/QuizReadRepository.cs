using Games.Application.QuizManagement;
using Games.Application.QuizPlaying;
using Games.Application.Quizzes;
using Games.Domain.Quizzes;
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

    public async Task<PageResult<AvailableQuizDto>> GetAvailableAsync(Guid playerId, int page, int pageSize)
    {
        var quizzes = _dbContext.Quizzes
            .AsNoTracking()
            .Where(quiz => quiz.Status == QuizStatus.Published)
            .Where(quiz => !_dbContext.ChallengeCompletions
                .Any(completion => completion.PlayerId == playerId && completion.ChallengeId == quiz.Id))
            .OrderBy(quiz => quiz.Name)
            .ThenBy(quiz => quiz.Id);
        var totalCount = await quizzes.CountAsync();
        var items = await quizzes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(quiz => new AvailableQuizDto(
                quiz.Id,
                quiz.Name,
                quiz.ExperiencePoints,
                quiz.Latitude != null && quiz.Longitude != null,
                _dbContext.QuizAttempts.Any(attempt => attempt.PlayerId == playerId && attempt.QuizId == quiz.Id)))
            .ToListAsync();

        return new PageResult<AvailableQuizDto>(items, totalCount);
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
