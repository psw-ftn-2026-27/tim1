using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizManagement;

public sealed class QuizManagementService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizManagementService(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(SaveQuizDto dto)
    {
        var quiz = new Quiz(dto.Name, dto.Instructions, dto.ExperiencePoints, MapQuestions(dto.Questions),
            dto.Latitude, dto.Longitude);
        _quizRepository.Add(quiz);
        await _unitOfWork.SaveChangesAsync();
        return quiz.Id;
    }

    public async Task UpdateAsync(Guid id, SaveQuizDto dto)
    {
        var quiz = await GetQuizAsync(id);

        quiz.Update(dto.Name, dto.Instructions, dto.ExperiencePoints, MapQuestions(dto.Questions),
            dto.Latitude, dto.Longitude);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PublishAsync(Guid id)
    {
        var quiz = await GetQuizAsync(id);
        quiz.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RetireAsync(Guid id)
    {
        var quiz = await GetQuizAsync(id);
        quiz.Retire();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid id)
    {
        var quiz = await GetQuizAsync(id);
        quiz.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var quiz = await GetQuizAsync(id);
        quiz.Delete();
        _quizRepository.Remove(quiz);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Quiz> GetQuizAsync(Guid id)
    {
        return await _quizRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Quiz {id} does not exist.");
    }

    private static QuizQuestion[] MapQuestions(IReadOnlyList<QuizQuestionInputDto>? questions)
    {
        return (questions ?? [])
            .Select(question => new QuizQuestion(
                question.Text ?? "",
                (question.Answers ?? []).Select(answer => new QuizAnswer(answer.Text ?? "", answer.IsCorrect)).ToArray(),
                question.Id))
            .ToArray();
    }
}
