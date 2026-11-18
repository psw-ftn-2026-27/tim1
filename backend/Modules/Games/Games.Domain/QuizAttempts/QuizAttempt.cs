using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.QuizAttempts;

public sealed class QuizAttempt : AggregateRoot
{
    private readonly List<GivenAnswer> _answers = [];

    public Guid PlayerId { get; private set; }
    public Guid QuizId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public IReadOnlyList<GivenAnswer> Answers => _answers.AsReadOnly();
    public int Points => _answers.Count(answer => answer.IsCorrect);

    private QuizAttempt() { }

    public QuizAttempt(Guid playerId, Guid quizId, DateTime now)
        : base(Guid.NewGuid())
    {
        PlayerId = playerId;
        QuizId = quizId;
        StartedAt = now;
    }

    public void AddAnswer(GivenAnswer answer)
    {
        if (CompletedAt is not null)
        {
            throw new DomainException("A completed attempt cannot take more answers.");
        }

        if (_answers.Any(given => given.QuestionId == answer.QuestionId))
        {
            throw new DomainException("This question has already been answered.");
        }

        _answers.Add(answer);
    }

    public void Complete(DateTime now)
    {
        CompletedAt = now;
    }
}
