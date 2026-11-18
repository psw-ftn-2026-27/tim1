using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club HikersClub = new(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarenja po Fruškoj gori.", ["https://example.com/planinari.jpg"]);
    public static readonly Club PhotographersClub = new(WellKnownUsers.Explorer, "Fotografi", "Klub ljubitelja fotografisanja gradskih znamenitosti.", ["https://example.com/fotografi.jpg", "https://example.com/fotografi-2.jpg"]);
    public static readonly Club SecondExplorersCyclingClub = new(WellKnownUsers.SecondExplorer, "Biciklisti", "Klub drugog osnivača za vožnju bicikla uz Dunav.", ["https://example.com/biciklisti.jpg"]);
    public static readonly Club RunnersClubWithSecondExplorersPendingRequest;
    public static readonly Club SwimmersClubWithSecondExplorersPendingInvitation;
    public static readonly Club ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest;

    static ClubSeed()
    {
        RunnersClubWithSecondExplorersPendingRequest = new(WellKnownUsers.Explorer, "Trkači", "Klub ljubitelja trčanja po keju.", ["https://example.com/trkaci.jpg"]);
        RunnersClubWithSecondExplorersPendingRequest.RequestMembership(WellKnownUsers.SecondExplorer);

        SwimmersClubWithSecondExplorersPendingInvitation = new(WellKnownUsers.Explorer, "Plivači", "Klub ljubitelja plivanja na Štrandu.", ["https://example.com/plivaci.jpg"]);
        SwimmersClubWithSecondExplorersPendingInvitation.InviteMember(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest = new(WellKnownUsers.Explorer, "Čitaoci", "Klub ljubitelja putopisa.", ["https://example.com/citaoci.jpg"]);
        ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.RequestMembership(WellKnownUsers.SecondExplorer);
        ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.AcceptMembershipRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.PromoteToModerator(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.RequestMembership(DemoUsers.ThirdExplorer);
    }

    public static Club[] All => [HikersClub, PhotographersClub, SecondExplorersCyclingClub, RunnersClubWithSecondExplorersPendingRequest, SwimmersClubWithSecondExplorersPendingInvitation, ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest];
}
