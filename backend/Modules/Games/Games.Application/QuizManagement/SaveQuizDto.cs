namespace Games.Application.QuizManagement;

public sealed record SaveQuizDto(
    string? Name,
    string? Instructions,
    int? ExperiencePoints,
    IReadOnlyList<QuizQuestionInputDto>? Questions);

public sealed record QuizQuestionInputDto(
    Guid? Id,
    string? Text,
    IReadOnlyList<QuizAnswerDto>? Answers);
