using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class QuizQuestion : Entity
{
    private readonly List<QuizAnswer> _answers = [];

    public string Text { get; private set; } = null!;
    public int Position { get; private set; }
    public IReadOnlyList<QuizAnswer> Answers => _answers.AsReadOnly();

    private QuizQuestion() { }

    public QuizQuestion(string text, IReadOnlyList<QuizAnswer> answers, Guid? id = null)
        : base(id ?? Guid.NewGuid())
    {
        if (answers.Count(answer => answer.IsCorrect) > 1)
        {
            throw new DomainException("Only one answer per question can be marked as correct.");
        }

        Text = text;
        _answers.AddRange(answers);
    }

    internal void SetPosition(int position)
    {
        Position = position;
    }

    internal void UpdateFrom(QuizQuestion question, int position)
    {
        Text = question.Text;
        Position = position;
        if (_answers.SequenceEqual(question.Answers))
        {
            return;
        }

        _answers.Clear();
        _answers.AddRange(question.Answers);
    }
}
