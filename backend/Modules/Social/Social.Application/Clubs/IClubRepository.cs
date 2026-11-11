using Social.Domain.Clubs;

namespace Social.Application.Clubs;

public interface IClubRepository
{
    Task<Club?> GetByIdAsync(Guid id);
    Task<Club?> GetByNameAsync(string name);
    void Add(Club club);
    void Remove(Club club);
}
