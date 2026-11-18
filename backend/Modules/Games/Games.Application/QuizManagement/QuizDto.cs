using Games.Domain.Quizzes;

namespace Games.Application.QuizManagement;

public sealed record QuizDto(
    Guid Id,
    string Name,
    string Instructions,
    int ExperiencePoints,
    QuizStatus Status,
    DateTime CreatedAt,
    IReadOnlyList<QuizQuestionDto> Questions,
    double? Latitude,
    double? Longitude);

public sealed record QuizQuestionDto(Guid Id, string Text, IReadOnlyList<QuizAnswerDto> Answers);

public sealed record QuizAnswerDto(string Text, bool IsCorrect);

public sealed record QuizSummaryDto(Guid Id, string Name, QuizStatus Status);
