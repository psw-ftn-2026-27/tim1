using Games.Application.Quizzes;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizManagement;

public sealed class QuizManagementQueries
{
    private readonly IQuizReadRepository _quizReadRepository;

    public QuizManagementQueries(IQuizReadRepository quizReadRepository)
    {
        _quizReadRepository = quizReadRepository;
    }

    public Task<PageResult<QuizSummaryDto>> GetAllAsync(int page, int pageSize)
    {
        return _quizReadRepository.GetAllAsync(Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
    }

    public async Task<QuizDto> GetByIdAsync(Guid id)
    {
        return await _quizReadRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Quiz {id} does not exist.");
    }
}
