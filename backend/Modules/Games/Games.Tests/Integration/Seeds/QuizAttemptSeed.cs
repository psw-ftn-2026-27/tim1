using Games.Domain.QuizAttempts;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class QuizAttemptSeed
{
    public static readonly QuizAttempt ExplorerInFountainTrail;
    public static readonly QuizAttempt ExplorerCompletedMonumentTrail;

    public static QuizAttempt[] All => [ExplorerInFountainTrail, ExplorerCompletedMonumentTrail];

    static QuizAttemptSeed()
    {
        ExplorerInFountainTrail = new QuizAttempt(WellKnownUsers.Explorer, QuizSeed.PublishedFountainTrail.Id, DateTime.UtcNow.AddMinutes(-5));
        ExplorerInFountainTrail.AddAnswer(new GivenAnswer(
            QuizSeed.PublishedFountainTrail.Questions[0].Id, 0, true, DateTime.UtcNow.AddMinutes(-4)));

        ExplorerCompletedMonumentTrail = new QuizAttempt(WellKnownUsers.Explorer, QuizSeed.PublishedMonumentTrail.Id, DateTime.UtcNow.AddMinutes(-5));
        ExplorerCompletedMonumentTrail.AddAnswer(new GivenAnswer(
            QuizSeed.PublishedMonumentTrail.Questions[0].Id, 0, true, DateTime.UtcNow.AddMinutes(-4)));
        ExplorerCompletedMonumentTrail.AddAnswer(new GivenAnswer(
            QuizSeed.PublishedMonumentTrail.Questions[1].Id, 0, false, DateTime.UtcNow.AddMinutes(-3)));
        ExplorerCompletedMonumentTrail.Complete(DateTime.UtcNow.AddMinutes(-3));
    }
}
