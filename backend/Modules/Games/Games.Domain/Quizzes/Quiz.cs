using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    private readonly List<QuizQuestion> _questions = [];

    public string Name { get; private set; } = null!;
    public string Instructions { get; private set; } = null!;
    public int ExperiencePoints { get; private set; }
    public QuizStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<QuizQuestion> Questions => _questions.AsReadOnly();

    private Quiz() { }

    public Quiz(string? name, string? instructions, int? experiencePoints, IReadOnlyList<QuizQuestion> questions)
        : base(Guid.NewGuid())
    {
        SetContent(name, instructions, experiencePoints, questions);
        Status = QuizStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string? name, string? instructions, int? experiencePoints, IReadOnlyList<QuizQuestion> questions)
    {
        if (Status != QuizStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be changed.");
        }

        SetContent(name, instructions, experiencePoints, questions);
    }

    public void Publish()
    {
        if (Status != QuizStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be published.");
        }

        if (_questions.Count == 0)
        {
            throw new DomainException("A quiz needs at least one question before it can be published.");
        }

        if (_questions.Any(question => question.Answers.Count < 2))
        {
            throw new DomainException("Each question needs at least two answers before the quiz can be published.");
        }

        if (_questions.Any(question => question.Answers.Count(answer => answer.IsCorrect) != 1))
        {
            throw new DomainException("Each question needs exactly one correct answer before the quiz can be published.");
        }

        Status = QuizStatus.Published;
    }

    private void SetContent(string? name, string? instructions, int? experiencePoints, IReadOnlyList<QuizQuestion> questions)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A quiz requires a name.");
        }

        if (string.IsNullOrWhiteSpace(instructions))
        {
            throw new DomainException("A quiz requires instructions.");
        }

        if (experiencePoints is null or < 0)
        {
            throw new DomainException("A quiz requires zero or more experience points.");
        }

        var questionIds = questions.Select(question => question.Id).ToHashSet();
        if (questionIds.Count != questions.Count)
        {
            throw new DomainException("A quiz cannot contain the same question more than once.");
        }

        Name = name;
        Instructions = instructions;
        ExperiencePoints = experiencePoints.Value;

        var existingQuestions = _questions.ToDictionary(question => question.Id);
        _questions.RemoveAll(question => !questionIds.Contains(question.Id));
        for (var position = 0; position < questions.Count; position++)
        {
            var question = questions[position];
            if (existingQuestions.TryGetValue(question.Id, out var existingQuestion))
            {
                existingQuestion.UpdateFrom(question, position);
            }
            else
            {
                var addedQuestion = new QuizQuestion(question.Text, question.Answers, question.Id);
                addedQuestion.SetPosition(position);
                _questions.Add(addedQuestion);
            }
        }

        _questions.Sort((left, right) => left.Position.CompareTo(right.Position));
    }
}
