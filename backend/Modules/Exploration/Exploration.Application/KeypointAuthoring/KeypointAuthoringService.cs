using Exploration.Application.Images;
using Exploration.Application.Tours;
using Exploration.Domain.Images;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.KeypointAuthoring;

public sealed class KeypointAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IImageStorage _imageStorage;
    private readonly IUnitOfWork _unitOfWork;

    public KeypointAuthoringService(
        ITourRepository tourRepository,
        IImageRepository imageRepository,
        IImageProcessor imageProcessor,
        IImageStorage imageStorage,
        IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _imageRepository = imageRepository;
        _imageProcessor = imageProcessor;
        _imageStorage = imageStorage;
        _unitOfWork = unitOfWork;
    }

    // The record is saved before the file is written, so a failure can leave a record
    // without a file, which the cleanup below removes later, but never a file without a
    // record. An expired image loses its file before its record, for the same reason.
    public async Task<Guid> UploadKeypointImageAsync(Guid userId, Stream content)
    {
        var processed = _imageProcessor.Process(content);
        var image = new Image(userId, processed.Width, processed.Height, processed.Content.Length);
        var expiredImages = (await _imageRepository.GetUnattachedAsync())
            .Where(unattached => unattached.IsExpired(DateTime.UtcNow))
            .ToList();

        foreach (var expired in expiredImages)
        {
            _imageStorage.Delete(expired.StoredFileName);
            _imageRepository.Remove(expired);
        }
        _imageRepository.Add(image);
        await _unitOfWork.SaveChangesAsync();

        await _imageStorage.SaveAsync(image.StoredFileName, processed.Content);

        return image.Id;
    }

    public async Task AddKeypointAsync(Guid tourId, Guid authorId, AddKeypointDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var image = await GetImageAsync(dto.ImageId);

        tour.AddKeypoint(dto.Name, dto.Description, dto.ImageId, dto.Secret, dto.PubliclyVisible,
            new Coordinate(dto.Location.Latitude, dto.Location.Longitude));
        image.AttachToKeypoint(authorId);
        await _unitOfWork.SaveChangesAsync();
    }

    // The kept image is already attached; a different image is attached, and the old one
    // stays attached (its record and file remain), because there is no detach yet.
    public async Task UpdateKeypointAsync(Guid tourId, Guid authorId, Guid keypointId, UpdateKeypointDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var currentImageId = tour.Keypoints.FirstOrDefault(keypoint => keypoint.Id == keypointId)?.ImageId;
        var newImage = currentImageId == dto.ImageId ? null : await GetImageAsync(dto.ImageId);

        tour.UpdateKeypoint(keypointId, dto.Name, dto.Description, dto.ImageId, dto.Secret,
            new Coordinate(dto.Location.Latitude, dto.Location.Longitude));
        newImage?.AttachToKeypoint(authorId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveKeypointAsync(Guid tourId, Guid authorId, Guid keypointId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.RemoveKeypoint(keypointId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetKeypointVisibilityAsync(Guid tourId, Guid authorId, Guid keypointId, SetKeypointVisibilityDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.SetKeypointVisibility(keypointId, dto.PubliclyVisible);
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

    private async Task<Image> GetImageAsync(Guid imageId)
    {
        var images = await _imageRepository.GetByIdsAsync([imageId]);
        var image = images.FirstOrDefault();
        if (image is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }
        return image;
    }
}
