namespace Games.Domain.QuizAttempts;

public sealed record GivenAnswer(Guid QuestionId, int AnswerIndex, bool IsCorrect, DateTime AnsweredAt);
