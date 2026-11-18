namespace Payment.Tests.Integration.Seeds;

/// <summary>
/// Users that exist in the identity schema of the test database: the Identity module creates
/// them at startup in Development and Reseed never touches that schema. Tests that need real
/// Identity users (recipient lookup, display names) use these instead of <c>WellKnownUsers</c>.
/// </summary>
internal static class DemoUsers
{
    public static readonly Guid Explorer = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid SecondExplorer = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid ThirdExplorer = Guid.Parse("20000000-0000-0000-0000-000000000003");
}
