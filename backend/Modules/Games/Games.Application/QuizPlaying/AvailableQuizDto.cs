namespace Games.Application.QuizPlaying;

public sealed record AvailableQuizDto(Guid Id, string Name, int ExperiencePoints, bool HasLocation, bool InProgress);
