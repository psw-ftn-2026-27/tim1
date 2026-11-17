using Shared.Tests;
using Social.Domain.FollowRequests;

namespace Social.Tests.Integration.Seeds;

internal static class FollowRequestSeed
{
    public static readonly FollowRequest ExplorerPendingToSecondExplorer = new(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
    public static readonly FollowRequest AdministratorPendingToExplorer = new(WellKnownUsers.Administrator, WellKnownUsers.Explorer);
    public static readonly FollowRequest SecondExplorerRejectedToExplorer = new(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);

    public static readonly FollowRequest DemoExplorerAcceptedToSecondExplorer = new(DemoUsers.Explorer, DemoUsers.SecondExplorer);
    public static readonly FollowRequest DemoExplorerPendingToThirdExplorer = new(DemoUsers.Explorer, DemoUsers.ThirdExplorer);
    public static readonly FollowRequest DemoExplorerRejectedToAdministrator = new(DemoUsers.Explorer, DemoUsers.Administrator);

    public static readonly FollowRequest DemoSecondExplorerAcceptedToExplorer = new(DemoUsers.SecondExplorer, DemoUsers.Explorer);
    public static readonly FollowRequest DemoThirdExplorerAcceptedToExplorer = new(DemoUsers.ThirdExplorer, DemoUsers.Explorer);
    public static readonly FollowRequest DemoSecondAdministratorPendingToExplorer = new(DemoUsers.SecondAdministrator, DemoUsers.Explorer);

    static FollowRequestSeed()
    {
        DemoSecondExplorerAcceptedToExplorer.Accept(DemoUsers.Explorer);
        DemoThirdExplorerAcceptedToExplorer.Accept(DemoUsers.Explorer);
        DemoExplorerAcceptedToSecondExplorer.Accept(DemoUsers.SecondExplorer);
        DemoExplorerRejectedToAdministrator.Reject(DemoUsers.Administrator);
        SecondExplorerRejectedToExplorer.Reject(WellKnownUsers.Explorer);
    }

    public static FollowRequest[] All => [ExplorerPendingToSecondExplorer, AdministratorPendingToExplorer, SecondExplorerRejectedToExplorer,
        DemoExplorerAcceptedToSecondExplorer, DemoExplorerPendingToThirdExplorer, DemoExplorerRejectedToAdministrator,
        DemoSecondExplorerAcceptedToExplorer, DemoThirdExplorerAcceptedToExplorer, DemoSecondAdministratorPendingToExplorer];
}
