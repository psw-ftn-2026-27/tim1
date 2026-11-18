namespace Games.Application.QuizPlaying;

public enum QuizPlayKind
{
    NotStarted,
    InProgress,
    Completed,
    Unavailable
}

public sealed record QuizPlayQuestionDto(Guid Id, string Text, IReadOnlyList<string> Answers);

public sealed record QuizPlayStateDto(
    Guid QuizId,
    QuizPlayKind Kind,
    string? QuizName,
    int? ExperiencePoints,
    double? Latitude,
    double? Longitude,
    QuizPlayQuestionDto? Question,
    int? QuestionNumber,
    int? QuestionCount,
    int? Points,
    bool? LastAnswerWasCorrect,
    int? CorrectAnswers,
    int? Percent,
    int? ExperiencePointsAwarded);
