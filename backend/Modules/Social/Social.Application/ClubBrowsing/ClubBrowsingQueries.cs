using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubBrowsing;

public sealed class ClubBrowsingQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubBrowsingQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    // P1: every club is listed, whoever founded it. P2 needs no filter, because a deleted
    // club is removed from the database.
    public Task<PageResult<ClubDto>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _clubReadRepository.GetAllAsync(page, pageSize);
    }

    public async Task<ClubDto> GetByIdAsync(Guid id)
    {
        var club = await _clubReadRepository.GetByIdAsync(id);
        if (club is null)
        {
            throw new NotFoundException($"Club {id} does not exist.");
        }

        return club;
    }
}
