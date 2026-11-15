namespace Identity.Contracts;

/// <summary>
/// Read-only view of platform users for other modules. DisplayName is currently the user's e-mail.
/// </summary>
public interface IIdentityApi
{
    /// <summary>
    /// Returns one page of users whose display name contains the text, ignoring letter case.
    /// Empty text matches every user. Page is at least 1, pageSize is clamped to 1..50.
    /// </summary>
    Task<IdentityUserPageDto> SearchUsersAsync(string text, int page, int pageSize);

    /// <summary>
    /// Returns the users found for the given IDs. IDs that belong to no user are omitted.
    /// </summary>
    Task<List<IdentityUserDto>> GetUsersAsync(IReadOnlyCollection<Guid> userIds);
}
