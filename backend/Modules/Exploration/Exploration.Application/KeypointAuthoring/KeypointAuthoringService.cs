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
        tour.SetLength(dto.LengthKm);
        image.AttachToKeypoint(authorId);
        await _unitOfWork.SaveChangesAsync();
    }

    // The kept image is already attached. A different image is attached and the old one is
    // detached, so the next image cleanup removes its record and file.
    public async Task UpdateKeypointAsync(Guid tourId, Guid authorId, Guid keypointId, UpdateKeypointDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var currentImageId = tour.Keypoints.FirstOrDefault(keypoint => keypoint.Id == keypointId)?.ImageId;
        var imageChanged = currentImageId is not null && currentImageId != dto.ImageId;
        var newImage = currentImageId == dto.ImageId ? null : await GetImageAsync(dto.ImageId);
        var oldImage = imageChanged ? await FindImageAsync(currentImageId!.Value) : null;

        tour.UpdateKeypoint(keypointId, dto.Name, dto.Description, dto.ImageId, dto.Secret,
            new Coordinate(dto.Location.Latitude, dto.Location.Longitude));
        tour.SetLength(dto.LengthKm);
        newImage?.AttachToKeypoint(authorId);
        oldImage?.Detach();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveKeypointAsync(Guid tourId, Guid authorId, Guid keypointId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var imageId = tour.Keypoints.FirstOrDefault(keypoint => keypoint.Id == keypointId)?.ImageId;
        var image = imageId is null ? null : await FindImageAsync(imageId.Value);

        tour.RemoveKeypoint(keypointId);
        image?.Detach();
        await _unitOfWork.SaveChangesAsync();
    }

    // Removing a keypoint sends no length, so the client measures the new route and sends it here.
    public async Task SetTourLengthAsync(Guid tourId, Guid authorId, SetTourLengthDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.SetLength(dto.LengthKm);
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

    private async Task<Image?> FindImageAsync(Guid imageId)
    {
        var images = await _imageRepository.GetByIdsAsync([imageId]);
        return images.FirstOrDefault();
    }

    private async Task<Image> GetImageAsync(Guid imageId)
    {
        var image = await FindImageAsync(imageId);
        if (image is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }
        return image;
    }
}
