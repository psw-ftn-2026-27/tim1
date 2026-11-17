using Identity.Core;
using Microsoft.AspNetCore.Identity;
using Shared.Tests;

namespace Identity.Tests.Integration.Seeds;

/// <summary>
/// Unlike feature-module seeds, these rows cannot come from a domain constructor:
/// Reseed inserts them straight into the database, bypassing UserManager, so the
/// normalized columns and the password hash must be filled in here.
/// </summary>
internal static class UserSeed
{
    public const string Password = "SuperSecret1!";

    private static readonly PasswordHasher<ApplicationUser> Hasher = new();

    public static readonly ApplicationUser Explorer = new()
    {
        Id = WellKnownUsers.Explorer,
        UserName = "explorer@test.com",
        NormalizedUserName = "EXPLORER@TEST.COM",
        Email = "explorer@test.com",
        NormalizedEmail = "EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "5f4a2f3e-8b1c-4d6e-9a0b-7c2d1e3f4a5b"
    };

    public static readonly ApplicationUser AnaJovic = new()
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
        UserName = "ana.jovic@test.com",
        NormalizedUserName = "ANA.JOVIC@TEST.COM",
        Email = "ana.jovic@test.com",
        NormalizedEmail = "ANA.JOVIC@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "a1b2c3d4-8b1c-4d6e-9a0b-7c2d1e3f4a5b"
    };

    public static readonly ApplicationUser MarkoNovak = new()
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
        UserName = "marko.novak@test.com",
        NormalizedUserName = "MARKO.NOVAK@TEST.COM",
        Email = "marko.novak@test.com",
        NormalizedEmail = "MARKO.NOVAK@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "b2c3d4e5-8b1c-4d6e-9a0b-7c2d1e3f4a5b"
    };

    public static readonly IdentityUserRole<Guid> ExplorerRole = new() { UserId = WellKnownUsers.Explorer, RoleId = RoleSeed.Explorer.Id };

    public static object[] All => [Explorer, AnaJovic, MarkoNovak, ExplorerRole];
}
