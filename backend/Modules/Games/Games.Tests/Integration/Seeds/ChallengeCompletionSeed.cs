using Games.Domain.ChallengeCompletions;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class ChallengeCompletionSeed
{
    public static readonly ChallengeCompletion ExplorerSolvedMonumentTrail = new(
        WellKnownUsers.Explorer, QuizSeed.PublishedMonumentTrail.Id, ChallengeType.Quiz, DateTime.UtcNow.AddMinutes(-3), 8);

    public static readonly ChallengeCompletion SecondExplorerSolvedSquareHunt = new(
        WellKnownUsers.SecondExplorer,
        LocationChallengeSeed.PublishedSquareHunt.Id,
        ChallengeType.Location,
        LocationChallengeAttemptSeed.SecondExplorersSolvedSquareAttempt.SolvedAt!.Value,
        LocationChallengeSeed.PublishedSquareHunt.ExperiencePoints);

    public static ChallengeCompletion[] All => [ExplorerSolvedMonumentTrail, SecondExplorerSolvedSquareHunt];
}
