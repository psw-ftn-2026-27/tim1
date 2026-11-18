using Games.Application.QuizManagement;
using Shared.Domain;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<PageResult<QuizSummaryDto>> GetAllAsync(int page, int pageSize);
    Task<QuizDto?> GetByIdAsync(Guid id);
}
