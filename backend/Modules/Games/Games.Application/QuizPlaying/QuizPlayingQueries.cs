using Games.Application.ChallengeCompletions;
using Games.Application.QuizAttempts;
using Games.Application.Quizzes;
using Games.Domain.ChallengeCompletions;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizPlaying;

public sealed class QuizPlayingQueries
{
    private readonly IQuizReadRepository _quizReadRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IChallengeCompletionRepository _completionRepository;
    private readonly QuizAnswering _answering;

    public QuizPlayingQueries(
        IQuizReadRepository quizReadRepository,
        IQuizRepository quizRepository,
        IQuizAttemptRepository attemptRepository,
        IChallengeCompletionRepository completionRepository,
        QuizAnswering answering)
    {
        _quizReadRepository = quizReadRepository;
        _quizRepository = quizRepository;
        _attemptRepository = attemptRepository;
        _completionRepository = completionRepository;
        _answering = answering;
    }

    public Task<PageResult<AvailableQuizDto>> GetAvailableAsync(Guid playerId, int page, int pageSize)
    {
        return _quizReadRepository.GetAvailableAsync(playerId, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
    }

    public async Task<QuizPlayStateDto> GetStateAsync(Guid playerId, Guid quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        var completion = await _completionRepository.GetByPlayerAndChallengeAsync(playerId, quizId);
        var attempt = await _attemptRepository.GetByPlayerAndQuizAsync(playerId, quizId);

        if (completion is not null)
        {
            return Completed(quizId, quiz, completion, attempt
                ?? throw new NotFoundException($"Quiz {quizId} has no attempt."));
        }

        if (quiz is null || quiz.Status != QuizStatus.Published)
        {
            return new QuizPlayStateDto(quizId, QuizPlayKind.Unavailable,
                null, null, null, null, null, null, null, null, null, null, null, null);
        }

        if (attempt is not null)
        {
            return InProgress(quiz, attempt);
        }

        return new QuizPlayStateDto(quizId, QuizPlayKind.NotStarted,
            quiz.Name, quiz.ExperiencePoints, quiz.Latitude, quiz.Longitude,
            null, null, null, null, null, null, null, null);
    }

    private QuizPlayStateDto InProgress(Quiz quiz, QuizAttempt attempt)
    {
        var current = _answering.CurrentQuestion(attempt, quiz);
        var question = current is null
            ? null
            : new QuizPlayQuestionDto(current.Id, current.Text, current.Answers.Select(answer => answer.Text).ToList());

        return new QuizPlayStateDto(quiz.Id, QuizPlayKind.InProgress,
            quiz.Name, quiz.ExperiencePoints, quiz.Latitude, quiz.Longitude,
            question, attempt.Answers.Count + 1, quiz.Questions.Count, attempt.Points, LastAnswerWasCorrect(attempt),
            null, null, null);
    }

    private static QuizPlayStateDto Completed(Guid quizId, Quiz? quiz, ChallengeCompletion completion, QuizAttempt attempt)
    {
        var total = attempt.Answers.Count;
        var percent = (int)Math.Round((double)attempt.Points * 100 / total, MidpointRounding.AwayFromZero);

        return new QuizPlayStateDto(quizId, QuizPlayKind.Completed,
            quiz?.Name, quiz?.ExperiencePoints, quiz?.Latitude, quiz?.Longitude,
            null, null, total, null, LastAnswerWasCorrect(attempt),
            attempt.Points, percent, completion.ExperiencePointsAwarded);
    }

    private static bool? LastAnswerWasCorrect(QuizAttempt attempt)
    {
        return attempt.Answers.OrderBy(answer => answer.AnsweredAt).LastOrDefault()?.IsCorrect;
    }
}
