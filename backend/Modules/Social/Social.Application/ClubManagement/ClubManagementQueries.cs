using Social.Application.Clubs;

namespace Social.Application.ClubManagement;

public sealed class ClubManagementQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubManagementQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId)
    {
        return _clubReadRepository.GetByOwnerAsync(ownerId);
    }
}
