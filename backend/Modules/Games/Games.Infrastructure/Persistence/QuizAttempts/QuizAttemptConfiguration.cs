using Games.Domain.QuizAttempts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.QuizAttempts;

internal static class QuizAttemptConfiguration
{
    public static void ConfigureQuizAttempts(this ModelBuilder modelBuilder)
    {
        ConfigureQuizAttempt(modelBuilder.Entity<QuizAttempt>());
    }

    private static void ConfigureQuizAttempt(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("QuizAttempts");
        builder.HasIndex(attempt => new { attempt.PlayerId, attempt.QuizId }).IsUnique();

        builder.OwnsMany(attempt => attempt.Answers, answers =>
        {
            answers.ToTable("GivenAnswers");
            answers.WithOwner().HasForeignKey("QuizAttemptId");
            answers.HasKey("QuizAttemptId", nameof(GivenAnswer.QuestionId));
        });

        builder.Navigation(attempt => attempt.Answers).HasField("_answers");
    }
}
