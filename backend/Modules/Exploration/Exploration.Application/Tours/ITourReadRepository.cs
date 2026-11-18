using Exploration.Application.KeypointAuthoring;
using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(TourSearchDto search, int page, int pageSize);
    Task<List<string>> GetPublishedTagsAsync();
    Task<TourManagementDto?> GetManagementAsync(Guid tourId, Guid authorId);
    Task<KeypointDto?> GetKeypointAsync(Guid tourId, Guid authorId, Guid keypointId);
}
