using Games.Application.QuizManagement;
using Games.Application.QuizPlaying;
using Shared.Domain;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<PageResult<QuizSummaryDto>> GetAllAsync(int page, int pageSize);
    Task<QuizDto?> GetByIdAsync(Guid id);
    Task<PageResult<AvailableQuizDto>> GetAvailableAsync(Guid playerId, int page, int pageSize);
}
