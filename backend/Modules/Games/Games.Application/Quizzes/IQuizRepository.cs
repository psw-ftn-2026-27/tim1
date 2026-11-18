using Games.Domain.Quizzes;

namespace Games.Application.Quizzes;

public interface IQuizRepository
{
    Task<Quiz?> GetByIdAsync(Guid id);
    void Add(Quiz quiz);
    void Remove(Quiz quiz);
}
