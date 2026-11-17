namespace Identity.Contracts;

/// <summary>
/// One page of users and the total number of users that matched the search.
/// </summary>
public sealed record IdentityUserPageDto(List<IdentityUserDto> Items, int TotalCount);
