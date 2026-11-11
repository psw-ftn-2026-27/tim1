using AutoMapper;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubManagement;

public sealed class ClubManagementService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ClubManagementService(IClubRepository clubRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ClubDto> CreateAsync(Guid ownerId, CreateClubDto dto)
    {
        var club = new Club(ownerId, dto.Name, dto.Description, dto.Images);
        var clubWithSameName = await _clubRepository.GetByNameAsync(club.Name);
        ClubNameUniqueness.EnsureNameIsFree(club, clubWithSameName);

        _clubRepository.Add(club);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ClubDto>(club);
    }

    public async Task UpdateAsync(Guid clubId, Guid ownerId, UpdateClubDto dto)
    {
        var club = await GetOwnedClubAsync(clubId, ownerId);

        club.Update(dto.Name, dto.Description, dto.Images);
        var clubWithSameName = await _clubRepository.GetByNameAsync(club.Name);
        ClubNameUniqueness.EnsureNameIsFree(club, clubWithSameName);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid clubId, Guid ownerId)
    {
        var club = await GetOwnedClubAsync(clubId, ownerId);

        _clubRepository.Remove(club);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetOwnedClubAsync(Guid clubId, Guid ownerId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null || club.OwnerId != ownerId)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }
}
