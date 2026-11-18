using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Payment.Contracts;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourAuthoring;

public sealed class TourAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentApi _paymentApi;

    public TourAuthoringService(ITourRepository tourRepository, IUnitOfWork unitOfWork, IPaymentApi paymentApi)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
        _paymentApi = paymentApi;
    }

    public async Task CreateAsync(Guid authorId, CreateTourDto dto)
    {
        var groupSize = new GroupSize(dto.GroupSize.Min, dto.GroupSize.Max);
        var tour = new Tour(authorId, dto.Name, dto.Description, dto.Difficulty, dto.Tags, groupSize);

        _tourRepository.Add(tour);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddTransportTimeAsync(Guid tourId, Guid authorId, TransportTimeDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.AddTransportTime(dto.Transport, dto.Minutes);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PublishAsync(Guid tourId, Guid authorId, PublishTourDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.Publish(dto.Price);
        await _paymentApi.RegisterProductAsync(new ProductRegistrationDto(ProductType.Tour, tour.Id, tour.Name, dto.Price, tour.AuthorId));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid tourId, Guid authorId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReactivateAsync(Guid tourId, Guid authorId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.Reactivate();
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.AuthorId != authorId)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return tour;
    }
}
