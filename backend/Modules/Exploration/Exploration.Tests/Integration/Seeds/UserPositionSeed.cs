using Exploration.Domain.UserPositions;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class UserPositionSeed
{
    public static readonly UserPosition ExplorersPosition = new(WellKnownUsers.Explorer, 45.25, 19.83);
}
