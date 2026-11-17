using Exploration.Application.TourPreferences;
using Exploration.Application.Tours;
using Exploration.Domain.TourPreferences;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourPreferenceEditing;

public sealed class TourPreferenceEditingService
{
    private readonly ITourPreferenceRepository _tourPreferenceRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourPreferenceEditingService(
        ITourPreferenceRepository tourPreferenceRepository,
        ITourRepository tourRepository,
        IUnitOfWork unitOfWork)
    {
        _tourPreferenceRepository = tourPreferenceRepository;
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SaveAsync(Guid userId, TourPreferenceDto dto)
    {
        var preference = await _tourPreferenceRepository.GetByUserIdAsync(userId);
        var publishedTours = await _tourRepository.GetPublishedAsync();

        AvailableTags.EnsureSelectable(dto.Tags, preference?.Tags ?? [], publishedTours);

        if (preference is null)
        {
            _tourPreferenceRepository.Add(new TourPreference(userId, dto.Difficulties, dto.Transport, dto.Tags, dto.OpensSearch));
        }
        else
        {
            preference.Change(dto.Difficulties, dto.Transport, dto.Tags, dto.OpensSearch);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId)
    {
        var preference = await _tourPreferenceRepository.GetByUserIdAsync(userId);
        if (preference is null)
        {
            throw new NotFoundException($"Tour preferences of user {userId} do not exist.");
        }

        _tourPreferenceRepository.Remove(preference);
        await _unitOfWork.SaveChangesAsync();
    }
}
