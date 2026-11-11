using Exploration.Application.TourPreferences;
using Exploration.Domain.TourPreferences;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourPreferences;

internal sealed class TourPreferenceRepository : ITourPreferenceRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourPreferenceRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourPreference?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.TourPreferences.FirstOrDefaultAsync(preference => preference.UserId == userId);
    }

    public void Add(TourPreference preference)
    {
        _dbContext.TourPreferences.Add(preference);
    }

    public void Remove(TourPreference preference)
    {
        _dbContext.TourPreferences.Remove(preference);
    }
}
