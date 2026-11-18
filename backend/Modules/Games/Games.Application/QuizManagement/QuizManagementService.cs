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
        var quiz = new Quiz(dto.Name, dto.Instructions, dto.ExperiencePoints, MapQuestions(dto.Questions));
        _quizRepository.Add(quiz);
        await _unitOfWork.SaveChangesAsync();
        return quiz.Id;
    }

    public async Task UpdateAsync(Guid id, SaveQuizDto dto)
    {
        var quiz = await _quizRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Quiz {id} does not exist.");

        quiz.Update(dto.Name, dto.Instructions, dto.ExperiencePoints, MapQuestions(dto.Questions));
        await _unitOfWork.SaveChangesAsync();
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
