using Exploration.Application.TourPreferences;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourPreferences;

internal sealed class TourPreferenceReadRepository : ITourPreferenceReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourPreferenceReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourPreferenceDto?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.TourPreferences
            .Where(preference => preference.UserId == userId)
            .Select(preference => new TourPreferenceDto(
                preference.Difficulties,
                preference.Transport,
                preference.Tags,
                preference.OpensSearch))
            .FirstOrDefaultAsync();
    }
}
