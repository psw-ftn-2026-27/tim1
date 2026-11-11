using Shared.Domain;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<ClubDto?> GetByIdAsync(Guid id);
    Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId);
    Task<PageResult<ClubDto>> GetAllAsync(int page, int pageSize);
}
