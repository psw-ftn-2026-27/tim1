using Games.Domain.GeoPositions;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    private const double ReachMeters = 50;

    private readonly List<QuizQuestion> _questions = [];

    public string Name { get; private set; } = null!;
    public string Instructions { get; private set; } = null!;
    public int ExperiencePoints { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public QuizStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<QuizQuestion> Questions => _questions.AsReadOnly();

    private Quiz() { }

    public Quiz(string? name, string? instructions, int? experiencePoints, IReadOnlyList<QuizQuestion> questions,
        double? latitude = null, double? longitude = null)
        : base(Guid.NewGuid())
    {
        SetContent(name, instructions, experiencePoints, questions, latitude, longitude);
        Status = QuizStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string? name, string? instructions, int? experiencePoints, IReadOnlyList<QuizQuestion> questions,
        double? latitude = null, double? longitude = null)
    {
        if (Status != QuizStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be changed.");
        }

        SetContent(name, instructions, experiencePoints, questions, latitude, longitude);
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

    public void Retire()
    {
        if (Status != QuizStatus.Published)
        {
            throw new DomainException("Only a published quiz can be retired.");
        }

        Status = QuizStatus.Retired;
    }

    public void Archive()
    {
        if (Status != QuizStatus.Retired)
        {
            throw new DomainException("Only a retired quiz can be archived.");
        }

        Status = QuizStatus.Archived;
    }

    public void Delete()
    {
        if (Status is not (QuizStatus.Draft or QuizStatus.Archived))
        {
            throw new DomainException("Only a draft or archived quiz can be deleted.");
        }
    }

    public bool IsWithinReach(GeoPosition position)
    {
        if (Latitude is null || Longitude is null)
        {
            return true;
        }

        return new GeoPosition(Latitude.Value, Longitude.Value).DistanceTo(position) <= ReachMeters;
    }

    public int ExperiencePointsFor(int correctAnswers)
    {
        return (int)Math.Round((double)ExperiencePoints * correctAnswers / _questions.Count, MidpointRounding.AwayFromZero);
    }

    private void SetContent(string? name, string? instructions, int? experiencePoints,
        IReadOnlyList<QuizQuestion> questions, double? latitude, double? longitude)
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

        ValidateLocation(latitude, longitude);

        Name = name;
        Instructions = instructions;
        ExperiencePoints = experiencePoints.Value;
        Latitude = latitude;
        Longitude = longitude;

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

    private static void ValidateLocation(double? latitude, double? longitude)
    {
        if (latitude.HasValue != longitude.HasValue)
        {
            throw new DomainException("Provide both latitude and longitude or leave both empty.");
        }

        if (latitude is double latitudeValue && (!double.IsFinite(latitudeValue) || latitudeValue is < -90 or > 90))
        {
            throw new DomainException("Latitude must be a finite number between -90 and 90 degrees.");
        }

        if (longitude is double longitudeValue && (!double.IsFinite(longitudeValue) || longitudeValue is < -180 or > 180))
        {
            throw new DomainException("Longitude must be a finite number between -180 and 180 degrees.");
        }
    }
}
