using Microsoft.EntityFrameworkCore;
using Social.Application.ClubManagement;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal sealed class ClubReadRepository : IClubReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ClubReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId)
    {
        return ProjectToDtos(_dbContext.Clubs.Where(club => club.OwnerId == ownerId)).ToListAsync();
    }

    private static IQueryable<ClubDto> ProjectToDtos(IQueryable<Club> clubs)
    {
        return clubs.Select(club => new ClubDto(club.Id, club.OwnerId, club.Name, club.Description, club.Images));
    }
}
