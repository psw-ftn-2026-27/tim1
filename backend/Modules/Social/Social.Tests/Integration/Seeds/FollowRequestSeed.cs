using Shared.Tests;
using Social.Domain.FollowRequests;

namespace Social.Tests.Integration.Seeds;

internal static class FollowRequestSeed
{
    public static readonly FollowRequest ExplorerPendingToSecondExplorer = new(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
    public static readonly FollowRequest AdministratorPendingToExplorer = new(WellKnownUsers.Administrator, WellKnownUsers.Explorer);

    public static FollowRequest[] All => [ExplorerPendingToSecondExplorer, AdministratorPendingToExplorer];
}
