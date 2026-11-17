namespace Identity.Contracts;

/// <summary>
/// A user as other modules see them. DisplayName is currently the user's e-mail.
/// </summary>
public sealed record IdentityUserDto(Guid Id, string DisplayName);
