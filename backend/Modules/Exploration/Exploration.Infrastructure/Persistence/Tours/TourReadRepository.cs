using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Tours.Where(tour => tour.AuthorId == authorId)).ToListAsync();
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(TourSearchDto search, int page, int pageSize)
    {
        var published = _dbContext.Tours.Where(tour => tour.Status == TourStatus.Published);
        published = ApplyFilters(published, search);
        published = ApplySort(published, search.Sort);

        var totalCount = await published.CountAsync();
        var items = await ProjectToDtos(published)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    public Task<List<string>> GetPublishedTagsAsync()
    {
        return _dbContext.Tours
            .Where(tour => tour.Status == TourStatus.Published)
            .SelectMany(tour => tour.Tags)
            .Distinct()
            .OrderBy(tag => tag)
            .ToListAsync();
    }

    private static IQueryable<Tour> ApplyFilters(IQueryable<Tour> tours, TourSearchDto search)
    {
        if (!string.IsNullOrWhiteSpace(search.Title))
        {
            var title = search.Title.ToLower();
            tours = tours.Where(tour => tour.Name.ToLower().Contains(title));
        }

        if (search.Difficulties.Count > 0)
        {
            tours = tours.Where(tour => search.Difficulties.Contains(tour.Difficulty));
        }

        if (search.Transports.Count > 0)
        {
            tours = tours.Where(tour => tour.TransportTimes.Any(time => search.Transports.Contains(time.Transport)));
        }

        foreach (var tag in search.Tags)
        {
            tours = tours.Where(tour => tour.Tags.Any((t => t.ToLower().Equals(tag.ToLower()))));
        }

        return tours;
    }

    private static IQueryable<Tour> ApplySort(IQueryable<Tour> tours, TourSortOrder sort)
    {
        return sort switch
        {
            TourSortOrder.DifficultyAscending => tours
                .OrderBy(tour =>
                    tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt),
            TourSortOrder.DifficultyDescending => tours
                .OrderByDescending(tour =>
                    tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt),
            _ => tours.OrderByDescending(tour => tour.PublishedAt)
        };
    }

    private static IQueryable<TourDto> ProjectToDtos(IQueryable<Tour> tours)
    {
        return tours
            .Select(tour => new TourDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Status,
                tour.PublishedAt,
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList()));
    }
}