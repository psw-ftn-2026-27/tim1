namespace Exploration.Contracts;

/// <summary>
/// What Exploration offers to other modules.
/// </summary>
public interface IExplorationApi
{
    /// <summary>
    /// Returns the user's most recently saved position. Returns null when the user has
    /// no saved position. Reading failures propagate to the caller.
    /// </summary>
    Task<UserPositionDto?> GetCurrentPositionAsync(Guid userId);
}
