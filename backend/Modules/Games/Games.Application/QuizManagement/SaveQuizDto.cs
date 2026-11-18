namespace Games.Application.QuizManagement;

public sealed record SaveQuizDto(
    string? Name,
    string? Instructions,
    int? ExperiencePoints,
    IReadOnlyList<QuizQuestionInputDto>? Questions,
    double? Latitude = null,
    double? Longitude = null);

public sealed record QuizQuestionInputDto(
    Guid? Id,
    string? Text,
    IReadOnlyList<QuizAnswerDto>? Answers);
