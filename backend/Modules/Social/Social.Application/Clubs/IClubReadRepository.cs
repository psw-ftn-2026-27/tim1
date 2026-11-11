using Social.Application.ClubManagement;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId);
}
