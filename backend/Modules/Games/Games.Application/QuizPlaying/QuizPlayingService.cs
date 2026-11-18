using Games.Application.ChallengeCompletions;
using Games.Application.QuizAttempts;
using Games.Application.Quizzes;
using Games.Domain.GeoPositions;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizPlaying;

public sealed class QuizPlayingService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IChallengeCompletionRepository _completionRepository;
    private readonly QuizActivation _activation;
    private readonly QuizAnswering _answering;
    private readonly IUnitOfWork _unitOfWork;

    public QuizPlayingService(
        IQuizRepository quizRepository,
        IQuizAttemptRepository attemptRepository,
        IChallengeCompletionRepository completionRepository,
        QuizActivation activation,
        QuizAnswering answering,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _attemptRepository = attemptRepository;
        _completionRepository = completionRepository;
        _activation = activation;
        _answering = answering;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid playerId, Guid quizId, ActivateQuizDto dto)
    {
        var quiz = await GetQuizAsync(quizId);
        var attempt = await _attemptRepository.GetByPlayerAndQuizAsync(playerId, quizId);
        var completion = await _completionRepository.GetByPlayerAndChallengeAsync(playerId, quizId);
        var position = new GeoPosition(dto.Latitude, dto.Longitude);

        var newAttempt = _activation.Activate(playerId, quiz, position, attempt, completion, DateTime.UtcNow);
        if (newAttempt is not null)
        {
            _attemptRepository.Add(newAttempt);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AnswerAsync(Guid playerId, Guid quizId, AnswerQuestionDto dto)
    {
        var quiz = await GetQuizAsync(quizId);
        var attempt = await _attemptRepository.GetByPlayerAndQuizAsync(playerId, quizId)
            ?? throw new NotFoundException($"Quiz {quizId} has not been activated.");

        var completion = _answering.Answer(attempt, quiz, dto.QuestionId, dto.AnswerIndex, DateTime.UtcNow);
        if (completion is not null)
        {
            _completionRepository.Add(completion);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Quiz> GetQuizAsync(Guid quizId)
    {
        return await _quizRepository.GetByIdAsync(quizId)
            ?? throw new NotFoundException($"Quiz {quizId} does not exist.");
    }
}
