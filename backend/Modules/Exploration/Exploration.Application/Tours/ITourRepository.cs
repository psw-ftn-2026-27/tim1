using Exploration.Domain.Tours;

namespace Exploration.Application.Tours;

public interface ITourRepository
{
    Task<Tour?> GetByIdAsync(Guid id);
    Task<List<Tour>> GetPublishedAsync();
    void Add(Tour tour);
}
